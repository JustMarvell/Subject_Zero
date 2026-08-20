using System;
using System.IO;
using UnityEngine;

namespace SubjectZero.Telemetry
{
    /// <summary>
    /// Loads the shared stress-score weights/thresholds from StreamingAssets. The same
    /// JSON file is meant to be read by the Python pipeline too, so the rule-based
    /// baseline and the training label always agree on what "too_hard" means.
    /// Note: File.ReadAllText from StreamingAssets works on PC/Editor builds (our target)
    /// but would need UnityWebRequest instead on Android/WebGL if you ever port there.
    /// </summary>
    [Serializable]
    public class DDAWeights
    {
        public WeightSet weights;
        public ThresholdSet thresholds;

        [Serializable]
        public class WeightSet
        {
            public float death_rate;
            public float near_miss_rate;
            public float reaction_time_inverse;
            public float hide_ratio;
            public float movement_erraticism;
            public float resource_usage_rate;
        }

        [Serializable]
        public class ThresholdSet
        {
            public float too_easy_max;
            public float too_hard_min;
        }

        private const string RelativePath = "DDA/stress_weights.json";

        public static DDAWeights Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, RelativePath);
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<DDAWeights>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DDAWeights] Failed to load {path}: {e.Message}. Using hardcoded fallback.");
                return Default();
            }
        }

        private static DDAWeights Default()
        {
            return new DDAWeights
            {
                weights = new WeightSet
                {
                    death_rate = 0.25f,
                    near_miss_rate = 0.15f,
                    reaction_time_inverse = 0.15f,
                    hide_ratio = 0.15f,
                    movement_erraticism = 0.15f,
                    resource_usage_rate = 0.15f
                },
                thresholds = new ThresholdSet { too_easy_max = -0.33f, too_hard_min = 0.33f }
            };
        }
    }
}