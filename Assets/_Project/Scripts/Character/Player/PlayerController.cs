using UnityEngine;
using SubjectZero.Input;
using SubjectZero.Core;

namespace SubjectZero.Character.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public enum LocomotionMode { Idle, Walk, Sneak, Sprint }

        [Header("References")]
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private PlayerMovementConfig config;

        public PlayerInputReader InputReader => inputReader;
        public Transform CameraPivot => cameraPivot;
        public PlayerMovementConfig Config => config;
        public CharacterController CharacterController { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public PlayerStance Stance { get; private set; }

        public PlayerIdleState IdleState { get; private set; }
        public PlayerLocomotionState LocomotionState { get; private set; }

        public Vector3 MoveVelocity { get; set; }
        public LocomotionMode CurrentLocomotionMode { get; set; } = LocomotionMode.Idle;
        public bool IsHidden { get; set; }
        public bool IsFlashlightOn { get; set; }

        /// <summary>
        /// 0-1 noise level for the player's current movement, factoring in crouch.
        /// Not consumed by anything yet - this is the hook the entity AI's hearing
        /// system and the telemetry collector will both read from in later phases.
        /// </summary>
        public float CurrentNoiseLevel01
        {
            get
            {
                float baseNoise = CurrentLocomotionMode switch
                {
                    LocomotionMode.Sneak => config.sneakNoise,
                    LocomotionMode.Walk => config.walkNoise,
                    LocomotionMode.Sprint => config.sprintNoise,
                    _ => 0f
                };
                return Stance.IsCrouching ? baseNoise * config.crouchNoiseMultiplier : baseNoise;
            }
        }

        private float _verticalVelocity;

        private void Awake()
        {
            CharacterController = GetComponent<CharacterController>();
            StateMachine = new StateMachine();
            Stance = new PlayerStance(this);

            IdleState = new PlayerIdleState(this);
            LocomotionState = new PlayerLocomotionState(this);
        }

        private void Start()
        {
            StateMachine.ChangeState(IdleState);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable()
        {
            inputReader.EnableInput();
        }

        private void OnDisable()
        {
            inputReader.DisableInput();
        }

        private void Update()
        {
            // Debug.Log($"[PlayerController] Update running. Frame: {Time.frameCount}");

            if (inputReader.CrouchPressedThisFrame)
                Stance.ToggleCrouch();

            StateMachine.Tick();
            Stance.Tick(Time.deltaTime);
            ApplyGravity();
            CharacterController.Move((MoveVelocity + Vector3.up * _verticalVelocity) * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (CharacterController.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = config.groundedStickForce;
            else
                _verticalVelocity += config.gravity * Time.deltaTime;
        }

        /// <summary>
        /// Converts raw move input into a world-space direction relative to the
        /// player body's own facing (not the camera - in first person they're the same yaw).
        /// </summary>
        public Vector3 GetPlayerRelativeDirection(Vector2 input)
        {
            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            return forward * input.y + right * input.x;
        }
    }
}