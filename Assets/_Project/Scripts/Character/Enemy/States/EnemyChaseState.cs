using SubjectZero.Core;
using SubjectZero.Telemetry;
using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    public class EnemyChaseState : IState
    {
        private readonly EnemyController _enemy;
        private float _loseSightTimer;
        private bool _caughtThisRun;

        public EnemyChaseState(EnemyController enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.Agent.speed = _enemy.Config.chaseSpeed;
            _loseSightTimer = 0f;
            _caughtThisRun = false;
        }

        public void Tick()
        {
            _enemy.Agent.SetDestination(_enemy.PlayerTransform.position);

            float distance = Vector3.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            if (distance <= _enemy.Config.catchDistance)
            {
                _caughtThisRun = true;
                _enemy.StateMachine.ChangeState(_enemy.LostState);
                _enemy.TriggerCatch();
                return;
            }

            if (_enemy.Perception.CanSeePlayer() || _enemy.Perception.CanHearPlayer())
            {
                _loseSightTimer = 0f;
                _enemy.LastKnownPlayerPosition = _enemy.PlayerTransform.position;
            }
            else
            {
                _loseSightTimer += Time.deltaTime;
                if (_loseSightTimer >= _enemy.Config.loseSightGraceTime)
                {
                    _enemy.StateMachine.ChangeState(_enemy.LostState);
                }
            }
        }

        public void FixedTick() { }

        public void Exit()
        {
            // A chase that ends WITHOUT a catch means the player evaded a live
            // pursuit - that's the near-miss signal from the telemetry design.
            if (!_caughtThisRun)
                TelemetryManager.Instance?.RecordNearMiss();
        }
    }
}