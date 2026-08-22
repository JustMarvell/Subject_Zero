using UnityEngine;
using SubjectZero.Character.Enemy;
using SubjectZero.Telemetry;

namespace SubjectZero.DDA
{
    /// <summary>
    /// Rule-based baseline DDA Controller. Reads the latest stress_score computed by
    /// TelemetryManager (via StressScoreCalculator - the same formula that generates
    /// training labels) and applies it to entity difficulty knobs. Adjustments are
    /// only ever applied while every tracked entity is in a "safe" state (Patrol or
    /// Lost) - never mid-chase - per the original design: changing difficulty while
    /// the player is actively being hunted would feel like the game cheating.
    ///
    /// This is a placeholder for the ML-driven controller. Once the classifier
    /// exists, it feeds the same ApplyDifficultyAdjustment(score) entry point on
    /// EnemyController - nothing here needs to change.
    ///
    /// Resource drop rate, spawn/encounter frequency, and fog/visibility knobs from
    /// the original design aren't wired up yet - they depend on systems (resources,
    /// spawners, fog) that don't exist yet. This controller only touches the two
    /// knobs that have something to act on right now.
    /// </summary>
    public class DDAController : MonoBehaviour
    {
        [SerializeField] private TelemetryManager telemetryManager;
        [SerializeField] private EnemyController[] enemies;

        private float? _pendingScore;

        private void OnEnable()
        {
            if (telemetryManager != null)
                telemetryManager.OnSampleLogged += HandleSampleLogged;
        }

        private void OnDisable()
        {
            if (telemetryManager != null)
                telemetryManager.OnSampleLogged -= HandleSampleLogged;
        }

        private void HandleSampleLogged(TelemetrySample sample)
        {
            if (telemetryManager != null && !telemetryManager.DDAEnabled) return;
            _pendingScore = sample.stress_score;
        }

        private void Update()
        {
            if (!_pendingScore.HasValue) return;

            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.IsInSafeState)
                    return; // wait - at least one entity is still actively engaged
            }

            float score = _pendingScore.Value;
            _pendingScore = null;

            foreach (var enemy in enemies)
            {
                enemy.ApplyDifficultyAdjustment(score);
                Debug.Log($"[DDAController] Adjusted {enemy.name}: score={score:F2} -> " +
                          $"chaseSpeed={enemy.CurrentChaseSpeed:F2}, visionRange={enemy.CurrentVisionRange:F2}");
            }
        }
    }
}