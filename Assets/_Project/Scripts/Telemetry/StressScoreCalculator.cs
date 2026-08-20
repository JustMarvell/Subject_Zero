using UnityEngine;

namespace SubjectZero.Telemetry
{
    /// <summary>
    /// The rule-based proxy formula. Serves three roles: the thesis's rule-based
    /// baseline model, the ground-truth label generator for training the classifier,
    /// and a runtime fallback before the ML model is wired in. One implementation only -
    /// no second copy in Python to drift out of sync with this one.
    /// </summary>
    public static class StressScoreCalculator
    {
        public static float ComputeScore(TelemetrySample sample, DDAWeights dda)
        {
            var w = dda.weights;

            // Reaction time inverted/normalized: faster reactions = lower stress
            // contribution. 2000ms ceiling is a placeholder - recalibrate against
            // real playtest reaction-time distributions once you have them.
            float reactionNorm = sample.avg_reaction_time > 0f
                ? Mathf.Clamp01(sample.avg_reaction_time / 2000f)
                : 0f;

            float score =
                w.death_rate * Normalize(sample.death_rate, 0f, 6f) +
                w.near_miss_rate * Normalize(sample.near_miss_rate, 0f, 10f) +
                w.reaction_time_inverse * reactionNorm +
                w.hide_ratio * sample.hide_ratio +
                w.movement_erraticism * Normalize(sample.movement_erraticism, 0f, 20f) +
                w.resource_usage_rate * Normalize(sample.resource_usage_rate, 0f, 6f);

            // Rescale [0,1] -> [-1,1] so 0 = balanced, matching the DDA knob formulas
            // from earlier (adjustment_score = P(too_hard) - P(too_easy)).
            return Mathf.Clamp(score * 2f - 1f, -1f, 1f);
        }

        public static string ComputeLabel(float score, DDAWeights dda)
        {
            if (score <= dda.thresholds.too_easy_max) return "too_easy";
            if (score >= dda.thresholds.too_hard_min) return "too_hard";
            return "balanced";
        }

        private static float Normalize(float value, float min, float max)
        {
            if (max <= min) return 0f;
            return Mathf.Clamp01((value - min) / (max - min));
        }
    }
}