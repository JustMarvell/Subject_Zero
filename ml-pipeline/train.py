"""
Trains the Random Forest classifier on labeled telemetry sessions. Labels
(difficulty_label) were computed in Unity by StressScoreCalculator at log
time - this script doesn't recompute them, it trains a classifier to
approximate/generalize that rule-based labeling from the raw feature values.
That framing matters for your evaluation chapter: test accuracy here measures
how well the classifier learned to reproduce and generalize the rule, not
"classifier vs. rule" in a competitive sense, since the rule generated the
ground truth by construction.

Run once you have a reasonable batch of playtest sessions in data/raw/.
"""
from pathlib import Path

import joblib
from sklearn.ensemble import RandomForestClassifier
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

MODEL_OUTPUT_PATH = Path(__file__).parent / "data" / "processed" / "model.joblib"


def main():
    df = load_all_sessions()

    missing = [c for c in FEATURE_COLUMNS if c not in df.columns]
    if missing:
        raise ValueError(f"Missing expected feature columns in data: {missing}")

    X = df[FEATURE_COLUMNS]
    y = df["difficulty_label"]
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

    X_train, X_test = X.iloc[train_idx], X.iloc[test_idx]
    y_train, y_test = y.iloc[train_idx], y.iloc[test_idx]

    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X_train, y_train)

    y_pred = clf.predict(X_test)

    print("\n--- Classification report (held-out sessions) ---")
    print(classification_report(y_test, y_pred, zero_division=0))

    print("--- Confusion matrix ---")
    print(f"Classes: {clf.classes_}")
    print(confusion_matrix(y_test, y_pred, labels=clf.classes_))

    print("\n--- Feature importances ---")
    importances = sorted(zip(FEATURE_COLUMNS, clf.feature_importances_), key=lambda x: -x[1])
    for name, score in importances:
        print(f"  {name}: {score:.3f}")

    MODEL_OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    joblib.dump(clf, MODEL_OUTPUT_PATH)
    print(f"\nModel saved to {MODEL_OUTPUT_PATH}")


if __name__ == "__main__":
    main()