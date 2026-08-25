using System;

namespace SubjectZero.Telemetry
{
    /// <summary>
    /// One row of training/evaluation data. Matches the JSONL schema written to disk -
    /// this is also exactly the feature vector the ML classifier will eventually consume.
    /// </summary>
    [Serializable]
    public class TelemetrySample
    {
        public string session_id;
        public float session_time;
        public string zone;

        public float death_rate;
        public float near_miss_rate;
        public float avg_reaction_time;
        public float hide_ratio;
        public float movement_erraticism;
        public float resource_usage_rate;
        public float idle_ratio;

        public float stress_score;
        public float darkness_ratio;
        public float reading_ratio;
        public string difficulty_label;
    }
}