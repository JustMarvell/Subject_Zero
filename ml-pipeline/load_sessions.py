"""Loads all JSONL telemetry sessions from data/raw into a single DataFrame."""
import json
from pathlib import Path
import pandas as pd

RAW_DIR = Path(__file__).parent / "data" / "raw"


def load_all_sessions() -> pd.DataFrame:
    rows = []
    for file in sorted(RAW_DIR.glob("session_*.jsonl")):
        with open(file, "r") as f:
            for line_num, line in enumerate(f, 1):
                line = line.strip()
                if not line:
                    continue
                try:
                    rows.append(json.loads(line))
                except json.JSONDecodeError as e:
                    print(f"  Skipping malformed line {line_num} in {file.name}: {e}")

    if not rows:
        raise FileNotFoundError(
            f"No session files found in {RAW_DIR}. Copy .jsonl files from "
            f"Application.persistentDataPath/TelemetrySessions into this folder first."
        )

    df = pd.DataFrame(rows)
    print(f"Loaded {len(df)} samples from {df['session_id'].nunique()} session(s).")
    return df


if __name__ == "__main__":
    df = load_all_sessions()
    print(df.head())
    print("\nLabel distribution:")
    print(df["difficulty_label"].value_counts())
    print("\nSamples per zone:")
    print(df["zone"].value_counts())