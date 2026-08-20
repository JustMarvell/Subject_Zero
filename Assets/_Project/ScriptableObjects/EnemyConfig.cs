using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    /// <summary>
    /// Tunable entity behavior values - same rationale as PlayerMovementConfig.
    /// chaseSpeed and visionRange defaults match the DDA knob base values from the
    /// original design table (3.5 m/s, 8m) so this config doubles as the DDA
    /// controller's starting point once that system exists.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "SubjectZero/Enemy/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Movement Speeds (m/s)")]
        public float patrolSpeed = 1.5f;
        public float alertSpeed = 2.2f;
        public float searchSpeed = 2.0f;
        public float chaseSpeed = 3.5f;

        [Header("Vision")]
        public float visionRange = 8f;
        [Tooltip("Full cone angle in degrees, not half-angle.")]
        public float visionAngle = 90f;
        public float eyeHeight = 1.6f;
        [Tooltip("Layers that can block line of sight. Player is auto-excluded via tag check.")]
        public LayerMask visionBlockingMask = ~0;

        [Header("Hearing")]
        [Tooltip("Max hearing distance at full player noise (sprinting). Scales down with player's CurrentNoiseLevel01.")]
        public float hearingRangeMax = 15f;

        [Header("Detection Timing")]
        [Tooltip("Seconds of continuous clear sight needed in Alert before confirming and starting a Chase.")]
        public float confirmSightDuration = 0.6f;
        [Tooltip("Seconds spent investigating in Alert before giving up to Search if not confirmed.")]
        public float alertInvestigateTimeout = 5f;
        public float searchDuration = 8f;
        public float searchPointRadius = 5f;
        [Tooltip("Seconds of losing the player during Chase before giving up to Lost.")]
        public float loseSightGraceTime = 3f;
        public float lostStateDuration = 4f;

        [Header("Catch")]
        public float catchDistance = 1.0f;
    }
}