"""
Exports the trained sklearn classifier to ONNX for Unity's Inference Engine.
Two things matter here that are easy to get wrong - see the two notes below.
"""
import json
from pathlib import Path

import joblib
import numpy as np
import onnxruntime as ort
from skl2onnx import convert_sklearn
from skl2onnx.common.data_types import FloatTensorType

from train import FEATURE_COLUMNS

MODEL_PATH = Path(__file__).parent / "data" / "processed" / "model.joblib"
ONNX_OUTPUT_PATH = Path(__file__).parent / "data" / "processed" / "dda_model.onnx"
METADATA_OUTPUT_PATH = Path(__file__).parent / "data" / "processed" / "dda_model_metadata.json"


def main():
    clf = joblib.load(MODEL_PATH)
    n_features = len(FEATURE_COLUMNS)

    initial_type = [("float_input", FloatTensorType([None, n_features]))]

    # zipmap=False: without this, probability output comes back as a
    # list-of-dicts (ZipMap), which Unity's Inference Engine can't consume as
    # a plain tensor.
    onnx_model = convert_sklearn(
        clf,
        initial_types=initial_type,
        options={id(clf): {"zipmap": False}},
        target_opset=15,
    )

    ONNX_OUTPUT_PATH.parent.mkdir(parents=True, exist_ok=True)
    with open(ONNX_OUTPUT_PATH, "wb") as f:
        f.write(onnx_model.SerializeToString())

    class_order = list(clf.classes_)
    metadata = {"feature_order": FEATURE_COLUMNS, "class_order": class_order}
    with open(METADATA_OUTPUT_PATH, "w") as f:
        json.dump(metadata, f, indent=2)

    print(f"Exported ONNX model to {ONNX_OUTPUT_PATH}")
    print(f"Feature order (must match Unity's featureOrder field): {FEATURE_COLUMNS}")
    print(f"Class order (must match Unity's classOrder field):     {class_order}")

    print("\nONNX model outputs:")
    for out in onnx_model.graph.output:
        print(f"  name='{out.name}'")
    print("(copy the probability output's name into Unity's probabilityOutputName field)")

    validate(clf, ONNX_OUTPUT_PATH, n_features)


def validate(clf, onnx_path, n_features):
    """Runs the same random inputs through sklearn and the exported ONNX
    model and checks the probabilities match closely - catches conversion
    mismatches here instead of as a confusing in-game bug later."""
    rng = np.random.RandomState(0)
    sample_input = rng.rand(5, n_features).astype(np.float32)
    sklearn_probs = clf.predict_proba(sample_input)

    session = ort.InferenceSession(str(onnx_path))
    output_names = [o.name for o in session.get_outputs()]
    onnx_result = session.run(output_names, {"float_input": sample_input})

    onnx_probs = next((o for o in onnx_result if getattr(o, "shape", None) == sklearn_probs.shape), None)

    if onnx_probs is None:
        print("\nWARNING: could not locate a matching probability tensor in ONNX output - inspect manually.")
        return

    max_diff = np.abs(sklearn_probs - onnx_probs).max()
    print(f"\nMax probability difference (sklearn vs ONNX): {max_diff:.6f}")
    print("Validation passed." if max_diff < 1e-4 else "WARNING: outputs diverge more than expected.")


if __name__ == "__main__":
    main()