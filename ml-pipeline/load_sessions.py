"""Loads all JSONL telemetry sessions from data/raw into a single DataFrame."""
import json
from pathlib import Path
import pandas as pd

RAW_DIR = Path(__file__).parent / "data" / "raw"

def load_all_sessions() -> pd.DataFrame:
    rows = []
    for file in RAW_DIR.glob("session_*.jsonl"):
        with open(file, "r") as f:
            for line in f:
                line = line.strip()
                if line:
                    rows.append(json.loads(line))

    if not rows:
        raise FileNotFoundError(f"No session files found in {RAW_DIR}")

    df = pd.DataFrame(rows)
    print(f"Loaded {len(df)} samples from {df['session_id'].nunique()} sessions.")
    return df

if __name__ == "__main__":
    df = load_all_sessions()
    print(df.head())
    print(df["difficulty_label"].value_counts())