using UnityEngine;
using Unity.InferenceEngine;
using SubjectZero.Character.Enemy;
using SubjectZero.Telemetry;

namespace SubjectZero.DDA
{
    /// <summary>
    /// Single DDA authority. useMlModel toggles between the rule-based
    /// stress_score (baseline/control) and the trained classifier's output -
    /// flip one Inspector checkbox to A/B them. If the model fails to load or
    /// inference throws, it falls back to the rule-based score automatically.
    /// </summary>
    public class DDAController : MonoBehaviour
    {
        [SerializeField] private TelemetryManager telemetryManager;
        [SerializeField] private EnemyController[] enemies;

        [Header("ML Model (optional)")]
        [SerializeField] private bool useMlModel = false;
        [SerializeField] private ModelAsset modelAsset;
        [SerializeField] private string probabilityOutputName = "probabilities";
        [SerializeField] private string[] classOrder = { "balanced", "too_easy", "too_hard" };
        [SerializeField]
        private string[] featureOrder = {
            "death_rate", "near_miss_rate", "avg_reaction_time", "hide_ratio",
            "movement_erraticism", "resource_usage_rate", "idle_ratio", "darkness_ratio"
        };

        private DDAModelInference _mlModel;
        private float? _pendingScore;

        private void OnEnable()
        {
            if (telemetryManager != null)
                telemetryManager.OnSampleLogged += HandleSampleLogged;

            if (useMlModel)
                _mlModel = new DDAModelInference(modelAsset, probabilityOutputName, classOrder);
        }

        private void OnDisable()
        {
            if (telemetryManager != null)
                telemetryManager.OnSampleLogged -= HandleSampleLogged;

            _mlModel?.Dispose();
            _mlModel = null;
        }

        private void HandleSampleLogged(TelemetrySample sample)
        {
            if (telemetryManager != null && !telemetryManager.DDAEnabled) return;

            float score = sample.stress_score; // rule-based baseline - always the fallback

            if (useMlModel && _mlModel != null && _mlModel.IsReady)
                score = _mlModel.Predict(BuildFeatureVector(sample), sample.stress_score);

            _pendingScore = score;
        }

        private float[] BuildFeatureVector(TelemetrySample sample)
        {
            var values = new float[featureOrder.Length];
            for (int i = 0; i < featureOrder.Length; i++)
                values[i] = GetFeatureValue(sample, featureOrder[i]);
            return values;
        }

        private float GetFeatureValue(TelemetrySample sample, string featureName) => featureName switch
        {
            "death_rate" => sample.death_rate,
            "near_miss_rate" => sample.near_miss_rate,
            "avg_reaction_time" => sample.avg_reaction_time,
            "hide_ratio" => sample.hide_ratio,
            "movement_erraticism" => sample.movement_erraticism,
            "resource_usage_rate" => sample.resource_usage_rate,
            "idle_ratio" => sample.idle_ratio,
            "darkness_ratio" => sample.darkness_ratio,
            _ => 0f
        };

        private void Update()
        {
            if (!_pendingScore.HasValue) return;

            foreach (var enemy in enemies)
                if (enemy == null || !enemy.IsInSafeState) return;

            float score = _pendingScore.Value;
            _pendingScore = null;

            foreach (var enemy in enemies)
                enemy.ApplyDifficultyAdjustment(score);
        }
    }
}