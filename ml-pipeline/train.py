"""
Trains a lightweight MLP classifier on labeled telemetry sessions. Labels
(difficulty_label) were computed in Unity by StressScoreCalculator at log
time - this script doesn't recompute them, it trains a classifier to
approximate/generalize that rule-based labeling from the raw feature values.
That framing matters for your evaluation chapter: test accuracy here measures
how well the classifier learned to reproduce and generalize the rule, not
"classifier vs. rule" in a competitive sense, since the rule generated the
ground truth by construction.

Switched from RandomForestClassifier to a small MLP because Unity Sentis
can't execute TreeEnsembleClassifier ops - an MLP is just Linear/ReLU/Softmax,
which Sentis runs fine.

Note on normalization: unlike a Random Forest, an MLP is sensitive to feature
scale. Rather than exporting a separate scaler that Unity would need to
replicate exactly, this script folds (x - mean) / std into the FIRST layer's
weights after training, so the saved/exported model accepts raw, unscaled
features - same as before. No Unity-side changes are required.

Run once you have a reasonable batch of playtest sessions in data/raw/.
"""
from pathlib import Path

import numpy as np
import torch
import torch.nn as nn
from sklearn.metrics import classification_report, confusion_matrix
from sklearn.model_selection import GroupShuffleSplit

from load_sessions import load_all_sessions

# darkness_ratio added after the Zone2 blackout mechanic was built - not part
# of the original 7-feature design. Drop the last line to revert to the
# original 7 features if you'd rather keep the feature set as originally scoped.
FEATURE_COLUMNS = [
    "death_rate",
    "near_miss_rate",
    "avg_reaction_time",
    "hide_ratio",
    "movement_erraticism",
    "resource_usage_rate",
    "idle_ratio",
    "darkness_ratio",
]

# Alphabetical order on purpose - matches DDAController's default classOrder
# ({"balanced", "too_easy", "too_hard"}) in Unity. If you change this, update
# the Inspector field too, or better, just copy the value straight out of the
# metadata JSON that export_onnx.py generates.
CLASS_ORDER = ["balanced", "too_easy", "too_hard"]

MODEL_OUTPUT_PATH = Path(__file__).parent / "data" / "processed" / "model.pt"

# Small on purpose - this runs every time a telemetry sample is logged during
# play. Bump these only if validation accuracy actually needs it.
HIDDEN_SIZES = (16, 16)

N_EPOCHS = 300
LEARNING_RATE = 1e-3
WEIGHT_DECAY = 1e-4


class DDAClassifier(nn.Module):
    """Plain MLP: Linear -> ReLU, repeated, -> Linear logits.
    Softmax is applied outside this module (see export_onnx.py) so that
    training can use nn.CrossEntropyLoss, which expects raw logits."""

    def __init__(self, n_features, n_classes, hidden_sizes=HIDDEN_SIZES):
        super().__init__()
        layers = []
        in_dim = n_features
        for h in hidden_sizes:
            layers += [nn.Linear(in_dim, h), nn.ReLU()]
            in_dim = h
        layers += [nn.Linear(in_dim, n_classes)]
        self.net = nn.Sequential(*layers)

    def forward(self, x):
        return self.net(x)


def main():
    df = load_all_sessions()

    missing = [c for c in FEATURE_COLUMNS if c not in df.columns]
    if missing:
        raise ValueError(f"Missing expected feature columns in data: {missing}")

    unexpected = set(df["difficulty_label"].unique()) - set(CLASS_ORDER)
    if unexpected:
        raise ValueError(
            f"Found labels not in CLASS_ORDER: {unexpected}. Update CLASS_ORDER "
            f"here (and classOrder in Unity) to match."
        )

    X = df[FEATURE_COLUMNS].to_numpy(dtype=np.float32)
    label_to_idx = {c: i for i, c in enumerate(CLASS_ORDER)}
    y = df["difficulty_label"].map(label_to_idx).to_numpy()
    groups = df["session_id"]

    n_sessions = groups.nunique()
    if n_sessions < 5:
        print(
            f"WARNING: only {n_sessions} session(s) in the dataset. Results below "
            f"are not meaningful yet - this is a pipeline smoke test, not a real "
            f"evaluation. Re-run once you have a proper varied playtest batch.\n"
        )

    # Split by SESSION, not by row - see note above on data leakage.
    splitter = GroupShuffleSplit(n_splits=1, test_size=0.2, random_state=42)
    train_idx, test_idx = next(splitter.split(X, y, groups=groups))

    X_train, X_test = X[train_idx], X[test_idx]
    y_train, y_test = y[train_idx], y[test_idx]

    # Normalization stats computed on TRAIN split only (no test leakage).
    mean = X_train.mean(axis=0)
    std = X_train.std(axis=0)
    std[std < 1e-6] = 1.0  # guard against a constant feature in small datasets

    X_train_norm = (X_train - mean) / std
    X_test_norm = (X_test - mean) / std

    X_train_t = torch.tensor(X_train_norm, dtype=torch.float32)
    y_train_t = torch.tensor(y_train, dtype=torch.long)
    X_test_t = torch.tensor(X_test_norm, dtype=torch.float32)
    y_test_t = torch.tensor(y_test, dtype=torch.long)

    model = DDAClassifier(len(FEATURE_COLUMNS), len(CLASS_ORDER))
    optimizer = torch.optim.Adam(model.parameters(), lr=LEARNING_RATE, weight_decay=WEIGHT_DECAY)
    loss_fn = nn.CrossEntropyLoss()

    model.train()
    for epoch in range(N_EPOCHS):
        optimizer.zero_grad()
        logits = model(X_train_t)
        loss = loss_fn(logits, y_train_t)
        loss.backward()
        optimizer.step()

        if (epoch + 1) % 50 == 0 or epoch == 0:
            model.eval()
            with torch.no_grad():
                val_loss = loss_fn(model(X_test_t), y_test_t).item()
            model.train()
            print(f"epoch {epoch + 1:4d}/{N_EPOCHS}  train_loss={loss.item():.4f}  val_loss={val_loss:.4f}")

    model.eval()
    with torch.no_grad():
        y_pred = model(X_test_t).argmax(dim=1).numpy()

    print("\n--- Classification report (held-out sessions) ---")
    print(
        classification_report(
            y_test, y_pred, target_names=CLASS_ORDER, labels=range(len(CLASS_ORDER)), zero_division=0
        )
    )

    print("--- Confusion matrix ---")
    print(f"Classes: {CLASS_ORDER}")
    print(confusion_matrix(y_test, y_pred, labels=range(len(CLASS_ORDER))))

    # --- Fold normalization into the first layer ---
    # y = W1 @ ((x - mean)/std) + b1 = (W1/std) @ x + (b1 - W1 @ (mean/std))
    # Reference output captured BEFORE folding (on normalized input) so we
    # can sanity-check the folded model (on raw input) against it below.
    with torch.no_grad():
        reference_logits = model(X_test_t).clone()

        first_linear = model.net[0]
        std_t = torch.tensor(std, dtype=torch.float32)
        mean_t = torch.tensor(mean, dtype=torch.float32)
        W1 = first_linear.weight.clone()
        b1 = first_linear.bias.clone()
        first_linear.weight.copy_(W1 / std_t)
        first_linear.bias.copy_(b1 - (W1 * (mean_t / std_t)).sum(dim=1))

        folded_logits = model(torch.tensor(X_test, dtype=torch.float32))  # raw, unscaled input
        fold_diff = (reference_logits - folded_logits).abs().max().item()
        print(f"\nWeight-folding sanity check max diff: {fold_diff:.6f} (should be ~0)")
        if fold_diff > 1e-4:
            print("WARNING: folding introduced a mismatch larger than expected - investigate before exporting.")

    MODEL_OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    torch.save(
        {
            "state_dict": model.state_dict(),
            "n_features": len(FEATURE_COLUMNS),
            "n_classes": len(CLASS_ORDER),
            "hidden_sizes": HIDDEN_SIZES,
        },
        MODEL_OUTPUT_PATH,
    )
    print(f"\nModel saved to {MODEL_OUTPUT_PATH}")
    print("Normalization is folded into the first layer - the model (and the ONNX")
    print("export of it) accepts RAW feature values, matching Unity's featureOrder.")


if __name__ == "__main__":
    main()