"""
Exports the trained PyTorch MLP to ONNX for Unity's Inference Engine (Sentis).

Two things matter here that are easy to get wrong - see the two notes below.
"""
import json
from pathlib import Path

import numpy as np
import onnxruntime as ort
import torch

from train import CLASS_ORDER, FEATURE_COLUMNS, MODEL_OUTPUT_PATH, DDAClassifier

ONNX_OUTPUT_PATH = Path(__file__).parent / "data" / "processed" / "dda_model.onnx"
METADATA_OUTPUT_PATH = Path(__file__).parent / "data" / "processed" / "dda_model_metadata.json"

INPUT_NAME = "float_input"
PROBABILITY_OUTPUT_NAME = "probabilities"


class DDAClassifierWithSoftmax(torch.nn.Module):
    """Wraps the trained MLP so softmax is baked into the exported graph -
    without this, Unity would get raw logits back and have to soft-max them
    itself. Same reasoning as the old zipmap=False flag: we want the
    probability tensor Unity reads to already BE probabilities."""

    def __init__(self, base_model):
        super().__init__()
        self.base_model = base_model

    def forward(self, x):
        logits = self.base_model(x)
        return torch.softmax(logits, dim=1)


def main():
    checkpoint = torch.load(MODEL_OUTPUT_PATH, weights_only=False)

    n_features = len(FEATURE_COLUMNS)
    if checkpoint["n_features"] != n_features:
        raise ValueError(
            f"Checkpoint was trained with {checkpoint['n_features']} features, but "
            f"FEATURE_COLUMNS in train.py currently has {n_features}. Re-train first."
        )

    model = DDAClassifier(checkpoint["n_features"], checkpoint["n_classes"], checkpoint["hidden_sizes"])
    model.load_state_dict(checkpoint["state_dict"])
    model.eval()

    export_model = DDAClassifierWithSoftmax(model)
    export_model.eval()

    dummy_input = torch.randn(1, n_features, dtype=torch.float32)

    ONNX_OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    torch.onnx.export(
        export_model,
        dummy_input,
        str(ONNX_OUTPUT_PATH),
        input_names=[INPUT_NAME],
        output_names=[PROBABILITY_OUTPUT_NAME],
        dynamic_axes={INPUT_NAME: {0: "batch"}, PROBABILITY_OUTPUT_NAME: {0: "batch"}},
        opset_version=15,
    )

    metadata = {"feature_order": FEATURE_COLUMNS, "class_order": CLASS_ORDER}
    with open(METADATA_OUTPUT_PATH, "w") as f:
        json.dump(metadata, f, indent=2)

    print(f"Exported ONNX model to {ONNX_OUTPUT_PATH}")
    print(f"Feature order (must match Unity's featureOrder field): {FEATURE_COLUMNS}")
    print(f"Class order (must match Unity's classOrder field):     {CLASS_ORDER}")

    print("\nONNX model outputs:")
    for out_name in [PROBABILITY_OUTPUT_NAME]:
        print(f"  name='{out_name}'")
    print("(copy the probability output's name into Unity's probabilityOutputName field)")

    validate(export_model, ONNX_OUTPUT_PATH, n_features)


def validate(export_model, onnx_path, n_features):
    """Runs the same random RAW-feature inputs through PyTorch and the
    exported ONNX model and checks the probabilities match closely - catches
    conversion mismatches here instead of as a confusing in-game bug later."""
    rng = np.random.RandomState(0)
    # Features aren't [0,1]-bounded (e.g. avg_reaction_time), so sample a
    # wider range than the old RandomForest validation used.
    sample_input = (rng.rand(5, n_features).astype(np.float32) - 0.5) * 10

    with torch.no_grad():
        torch_probs = export_model(torch.tensor(sample_input)).numpy()

    session = ort.InferenceSession(str(onnx_path))
    output_names = [o.name for o in session.get_outputs()]
    onnx_result = session.run(output_names, {INPUT_NAME: sample_input})
    onnx_probs = onnx_result[output_names.index(PROBABILITY_OUTPUT_NAME)]

    max_diff = np.abs(torch_probs - onnx_probs).max()
    print(f"\nMax probability difference (PyTorch vs ONNX): {max_diff:.6f}")
    print("Validation passed." if max_diff < 1e-4 else "WARNING: outputs diverge more than expected.")


if __name__ == "__main__":
    main()