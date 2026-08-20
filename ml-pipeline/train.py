"""
Trains the Decision Tree / Random Forest classifier on labeled telemetry sessions.
difficulty_label was computed by Unity's StressScoreCalculator at log time - this
script doesn't recompute labels, it just trains on them. Run once you have a real
batch of playtest sessions in data/raw/.
"""
from load_sessions import load_all_sessions
from sklearn.model_selection import train_test_split
from sklearn.ensemble import RandomForestClassifier
from sklearn.metrics import classification_report, confusion_matrix

FEATURE_COLUMNS = [
    "death_rate", "near_miss_rate", "avg_reaction_time",
    "hide_ratio", "movement_erraticism", "resource_usage_rate", "idle_ratio",
]

def main():
    df = load_all_sessions()
    X = df[FEATURE_COLUMNS]
    y = df["difficulty_label"]

    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=0.2, random_state=42, stratify=y
    )

    clf = RandomForestClassifier(n_estimators=100, random_state=42)
    clf.fit(X_train, y_train)

    y_pred = clf.predict(X_test)
    print(classification_report(y_test, y_pred))
    print(confusion_matrix(y_test, y_pred))

    importances = sorted(zip(FEATURE_COLUMNS, clf.feature_importances_), key=lambda x: -x[1])
    print("\nFeature importances:")
    for name, score in importances:
        print(f"  {name}: {score:.3f}")

if __name__ == "__main__":
    main()