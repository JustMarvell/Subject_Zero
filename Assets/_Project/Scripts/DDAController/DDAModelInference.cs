using System;
using Unity.InferenceEngine;
using UnityEngine;

namespace SubjectZero.DDA
{
    public class DDAModelInference : IDisposable
    {
        private readonly Worker _worker;
        private readonly string _probabilityOutputName;
        private readonly int _tooEasyIndex;
        private readonly int _tooHardIndex;

        public bool IsReady { get; }

        public DDAModelInference(ModelAsset modelAsset, string probabilityOutputName, string[] classOrder)
        {
            _probabilityOutputName = probabilityOutputName;

            if (modelAsset == null)
            {
                Debug.LogWarning("[DDAModelInference] No model asset provided.");
                return;
            }

            _tooEasyIndex = Array.IndexOf(classOrder, "too_easy");
            _tooHardIndex = Array.IndexOf(classOrder, "too_hard");
            if (_tooEasyIndex < 0 || _tooHardIndex < 0)
            {
                Debug.LogError("[DDAModelInference] classOrder must include 'too_easy' and 'too_hard' - check the export metadata JSON.");
                return;
            }

            var model = ModelLoader.Load(modelAsset);
            _worker = new Worker(model, BackendType.CPU);
            IsReady = true;
        }

        public float Predict(float[] featureVector, float fallbackScore)
        {
            if (!IsReady) return fallbackScore;

            using var inputTensor = new Tensor<float>(new TensorShape(1, featureVector.Length), featureVector);

            try
            {
                _worker.Schedule(inputTensor);
                var output = _worker.PeekOutput(_probabilityOutputName) as Tensor<float>;
                if (output == null) return fallbackScore;

                float[] probs = output.DownloadToArray();
                return probs[_tooHardIndex] - probs[_tooEasyIndex];
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DDAModelInference] Inference failed: {e.Message}. Falling back to rule-based score.");
                return fallbackScore;
            }
        }

        public void Dispose() => _worker?.Dispose();
    }
}