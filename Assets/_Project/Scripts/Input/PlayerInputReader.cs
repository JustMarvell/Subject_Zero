using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SubjectZero.Input
{
    /// <summary>
    /// Reads the "Player" action map from an InputActionAsset and exposes the current
    /// values as plain properties/events. Deliberately does NOT rely on Unity's
    /// generated C# wrapper class for the .inputactions asset - it binds to actions
    /// by name instead, so nothing breaks if the asset is edited later.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerInputReader", menuName = "SubjectZero/Input/Player Input Reader")]
    public class PlayerInputReader : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";

        public event Action OnCrouchPressed;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintHeld { get; private set; }
        public bool SneakHeld { get; private set; }

        private InputActionMap _map;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _sprintAction;
        private InputAction _sneakAction;
        private InputAction _crouchAction;
        private bool _initialized;

        private void OnEnable()
        {
            if (inputActions == null)
            {
                Debug.LogError($"[{name}] No InputActionAsset assigned.", this);
                return;
            }

            _map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);

            if (!_initialized)
            {
                _moveAction = _map.FindAction("Move");
                _lookAction = _map.FindAction("Look");
                _sprintAction = _map.FindAction("Sprint");
                _sneakAction = _map.FindAction("Sneak");
                _crouchAction = _map.FindAction("Crouch");

                _moveAction.performed += OnMoveChanged;
                _moveAction.canceled += OnMoveChanged;

                _lookAction.performed += OnLookChanged;
                _lookAction.canceled += OnLookChanged;

                _sprintAction.performed += OnSprintChanged;
                _sprintAction.canceled += OnSprintChanged;

                _sneakAction.performed += OnSneakChanged;
                _sneakAction.canceled += OnSneakChanged;

                _crouchAction.performed += OnCrouchAction;

                _initialized = true;
            }

            _map.Enable();
        }

        private void OnDisable()
        {
            _map?.Disable();
        }

        private void OnMoveChanged(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
        private void OnLookChanged(InputAction.CallbackContext ctx) => LookInput = ctx.ReadValue<Vector2>();
        private void OnSprintChanged(InputAction.CallbackContext ctx) => SprintHeld = ctx.performed;
        private void OnSneakChanged(InputAction.CallbackContext ctx) => SneakHeld = ctx.performed;
        private void OnCrouchAction(InputAction.CallbackContext ctx) => OnCrouchPressed?.Invoke();
    }
}