using SubjectZero.Core;
using UnityEngine;

namespace SubjectZero.Character.Player
{
    public class PlayerIdleState : IState
    {
        private readonly PlayerController _player;
        public PlayerIdleState(PlayerController player) => _player = player;

        public void Enter()
        {
            _player.MoveVelocity = Vector3.zero;
            _player.CurrentLocomotionMode = PlayerController.LocomotionMode.Idle;
        }

        public void Tick()
        {
            if (_player.InputReader.MoveInput.sqrMagnitude > 0.01f)
                _player.StateMachine.ChangeState(_player.LocomotionState);
        }

        public void FixedTick() { }
        public void Exit() { }
    }
}