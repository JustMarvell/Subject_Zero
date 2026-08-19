using SubjectZero.Core;
using UnityEngine;

namespace SubjectZero.Character.Player
{
    public class PlayerLocomotionState : IState
    {
        private readonly PlayerController _player;
        public PlayerLocomotionState(PlayerController player) => _player = player;

        public void Enter() { }

        public void Tick()
        {
            Vector2 rawInput = _player.InputReader.MoveInput;
            if (rawInput.sqrMagnitude < 0.01f)
            {
                _player.StateMachine.ChangeState(_player.IdleState);
                return;
            }

            Vector2 clampedInput = Vector2.ClampMagnitude(rawInput, 1f);
            Vector3 moveDir = _player.GetPlayerRelativeDirection(clampedInput);

            PlayerController.LocomotionMode mode = DetermineMode();
            _player.CurrentLocomotionMode = mode;
            _player.MoveVelocity = moveDir * GetSpeedForMode(mode);
        }

        private PlayerController.LocomotionMode DetermineMode()
        {
            bool crouching = _player.Stance.IsCrouching;

            // Sprinting is disabled while crouched - standard convention, also keeps
            // the noise/detection story consistent (you can't sprint quietly).
            if (!crouching && _player.InputReader.SprintHeld)
                return PlayerController.LocomotionMode.Sprint;

            if (_player.InputReader.SneakHeld)
                return PlayerController.LocomotionMode.Sneak;

            return PlayerController.LocomotionMode.Walk;
        }

        private float GetSpeedForMode(PlayerController.LocomotionMode mode)
        {
            var config = _player.Config;
            float baseSpeed = mode switch
            {
                PlayerController.LocomotionMode.Sprint => config.sprintSpeed,
                PlayerController.LocomotionMode.Sneak => config.sneakSpeed,
                _ => config.walkSpeed
            };
            return _player.Stance.IsCrouching ? baseSpeed * config.crouchSpeedMultiplier : baseSpeed;
        }

        public void FixedTick() { }

        public void Exit()
        {
            _player.MoveVelocity = Vector3.zero;
        }
    }
}