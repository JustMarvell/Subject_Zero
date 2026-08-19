using UnityEngine;

namespace SubjectZero.Character.Player
{
    /// <summary>
    /// All tunable player movement values live here rather than as hardcoded constants,
    /// so they can be swapped/versioned per playtest batch and (later) read/adjusted
    /// by the DDA Controller without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerMovementConfig", menuName = "SubjectZero/Player/Movement Config")]
    public class PlayerMovementConfig : ScriptableObject
    {
        [Header("Movement Speeds (m/s)")]
        public float walkSpeed = 2.0f;
        public float sneakSpeed = 1.0f;
        public float sprintSpeed = 4.5f;
        [Tooltip("Multiplier applied on top of the base speed while crouching. Sprint is disabled while crouching.")]
        public float crouchSpeedMultiplier = 0.6f;

        [Header("Crouch / Stance")]
        public float standingHeight = 1.8f;
        public float crouchingHeight = 1.0f;
        public float standingCenterY = 0.9f;
        public float crouchingCenterY = 0.5f;
        public float cameraStandingHeight = 1.65f;
        public float cameraCrouchingHeight = 0.95f;
        [Tooltip("Higher = snappier crouch transition.")]
        public float crouchTransitionSpeed = 8f;
        [Tooltip("Layers checked above the player's head before allowing them to stand back up.")]
        public LayerMask obstructionMask = ~0;

        [Header("Gravity")]
        public float gravity = -20f;
        public float groundedStickForce = -2f;

        [Header("Look")]
        public float lookSensitivityX = 180f;
        public float lookSensitivityY = 120f;
        public float pitchClampMin = -80f;
        public float pitchClampMax = 80f;

        [Header("Noise (0-1) - for future AI hearing / telemetry use")]
        [Range(0f, 1f)] public float sneakNoise = 0.1f;
        [Range(0f, 1f)] public float walkNoise = 0.5f;
        [Range(0f, 1f)] public float sprintNoise = 1.0f;
        [Range(0f, 1f)] public float crouchNoiseMultiplier = 0.5f;
    }
}