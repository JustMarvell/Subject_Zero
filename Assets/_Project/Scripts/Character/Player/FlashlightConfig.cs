using UnityEngine;

namespace SubjectZero.Character.Player
{
    [CreateAssetMenu(fileName = "FlashlightConfig", menuName = "SubjectZero/Player/Flashlight Config")]
    public class FlashlightConfig : ScriptableObject
    {
        [Header("Battery")]
        [Tooltip("Total seconds of continuous use on a full battery.")]
        public float capacitySeconds = 180f;
        [Tooltip("0-1. Below this, the light starts flickering as a warning.")]
        [Range(0f, 1f)] public float lowBatteryThreshold = 0.2f;
        [Tooltip("How strongly the light flickers near empty (intensity jitter amount).")]
        public float flickerAmount = 0.15f;

        [Header("Light")]
        public float lightIntensity = 3.5f;
    }
}