using SubjectZero.Core;
using SubjectZero.Telemetry;
using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    /// <summary>
    /// "Heard/glimpsed something, not confirmed" - investigates the last known
    /// position. Confirms into Chase only after sustained clear sight, or gives
    /// up to Search after a timeout.
    /// </summary>
    public class EnemyAlertState : IState
    {
        private readonly EnemyController _enemy;
        private float _timer;
        private float _sightConfirmTimer;

        public EnemyAlertState(EnemyController enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.Agent.speed = _enemy.Config.alertSpeed;
            _enemy.Agent.SetDestination(_enemy.LastKnownPlayerPosition);
            _timer = 0f;
            _sightConfirmTimer = 0f;

            // This is the "sudden threat" moment for reaction-time telemetry -
            // the first time the entity registers the player.
            TelemetryManager.Instance?.ArmReactionWindow();
        }

        public void Tick()
        {
            _timer += Time.deltaTime;

            if (_enemy.Perception.CanSeePlayer())
            {
                _enemy.LastKnownPlayerPosition = _enemy.PlayerTransform.position;
                _enemy.Agent.SetDestination(_enemy.LastKnownPlayerPosition);
                _sightConfirmTimer += Time.deltaTime;

                if (_sightConfirmTimer >= _enemy.Config.confirmSightDuration)
                {
                    _enemy.StateMachine.ChangeState(_enemy.ChaseState);
                    return;
                }
            }
            else
            {
                _sightConfirmTimer = 0f;
                if (_enemy.Perception.CanHearPlayer())
                {
                    _enemy.LastKnownPlayerPosition = _enemy.PlayerTransform.position;
                    _enemy.Agent.SetDestination(_enemy.LastKnownPlayerPosition);
                }
            }

            if (_timer >= _enemy.Config.alertInvestigateTimeout)
                _enemy.StateMachine.ChangeState(_enemy.SearchState);
        }

        public void FixedTick() { }
        public void Exit() { }
    }
}