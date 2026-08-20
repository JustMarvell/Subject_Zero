using SubjectZero.Core;
using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    public class EnemyLostState : IState
    {
        private readonly EnemyController _enemy;
        private float _timer;

        public EnemyLostState(EnemyController enemy) => _enemy = enemy;

        public void Enter()
        {
            _timer = 0f;
            _enemy.Agent.ResetPath();
        }

        public void Tick()
        {
            _timer += Time.deltaTime;

            if (_enemy.Perception.CanSeePlayer())
            {
                _enemy.LastKnownPlayerPosition = _enemy.PlayerTransform.position;
                _enemy.StateMachine.ChangeState(_enemy.ChaseState);
                return;
            }

            if (_timer >= _enemy.Config.lostStateDuration)
                _enemy.StateMachine.ChangeState(_enemy.PatrolState);
        }

        public void FixedTick() { }
        public void Exit() { }
    }
}