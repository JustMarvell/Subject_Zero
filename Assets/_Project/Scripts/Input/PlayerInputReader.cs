using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SubjectZero.Input
{
    /// <summary>
    /// Reads the "Player" action map from an InputActionAsset. Continuous values
    /// (Move, Look, Sprint, Sneak) are polled directly each frame rather than via
    /// performed/canceled callbacks - simpler, avoids event-subscription lifecycle
    /// pitfalls, and is the standard approach for per-frame axis input. Crouch stays
    /// press-detected since it's a discrete toggle, but polled too (WasPerformedThisFrame)
    /// rather than event-based, for consistency.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerInputReader", menuName = "SubjectZero/Input/Player Input Reader")]
    public class PlayerInputReader : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";

        private InputActionMap _map;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _sprintAction;
        private InputAction _sneakAction;
        private InputAction _crouchAction;
        private InputAction _interactAction;
        private InputAction _flashlightAction;
        private InputAction _pauseAction;

        public Vector2 MoveInput => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public Vector2 LookInput => _lookAction?.ReadValue<Vector2>() ?? Vector2.zero;
        public bool SprintHeld => _sprintAction != null && _sprintAction.IsPressed();
        public bool SneakHeld => _sneakAction != null && _sneakAction.IsPressed();
        public bool CrouchPressedThisFrame => _crouchAction != null && _crouchAction.WasPerformedThisFrame();
        public bool InteractPressedThisFrame => _interactAction != null && _interactAction.WasPerformedThisFrame();
        public bool FlashlightPressedThisFrame => _flashlightAction != null && _flashlightAction.WasPerformedThisFrame();
        public bool PausePressedThisFrame => _pauseAction != null && _pauseAction.WasPerformedThisFrame();

        private void OnEnable()
        {
            if (inputActions == null)
            {
                Debug.LogError($"[{name}] No InputActionAsset assigned.", this);
                return;
            }

            _map = inputActions.FindActionMap(actionMapName, throwIfNotFound: true);

            _moveAction = _map.FindAction("Move");
            _lookAction = _map.FindAction("Look");
            _sprintAction = _map.FindAction("Sprint");
            _sneakAction = _map.FindAction("Sneak");
            _crouchAction = _map.FindAction("Crouch");
            _interactAction = _map.FindAction("Interact");
            _flashlightAction = _map.FindAction("Flashlight");
            _pauseAction = _map.FindAction("Pause");

            if (_moveAction == null || _lookAction == null || _sprintAction == null || _sneakAction == null || _crouchAction == null || _interactAction == null || _flashlightAction == null || _pauseAction == null)
                Debug.LogError($"[PlayerInputReader] One or more actions not found in map '{actionMapName}'. Check names in the .inputactions asset.");
        }

        private void OnDisable() { }

        public void EnableInput() => _map?.Enable();
        public void DisableInput() => _map?.Disable();
    }
}