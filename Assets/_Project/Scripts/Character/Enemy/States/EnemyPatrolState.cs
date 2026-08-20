using SubjectZero.Core;
using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    public class EnemyPatrolState : IState
    {
        private readonly EnemyController _enemy;
        private int _waypointIndex;

        public EnemyPatrolState(EnemyController enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.Agent.speed = _enemy.Config.patrolSpeed;
            MoveToCurrentWaypoint();
        }

        public void Tick()
        {
            if (_enemy.Perception.CanSeePlayer() || _enemy.Perception.CanHearPlayer())
            {
                _enemy.LastKnownPlayerPosition = _enemy.PlayerTransform.position;
                _enemy.StateMachine.ChangeState(_enemy.AlertState);
                return;
            }

            if (_enemy.PatrolRoute != null && !_enemy.Agent.pathPending &&
                _enemy.Agent.remainingDistance < 0.5f)
            {
                _waypointIndex++;
                MoveToCurrentWaypoint();
            }
        }

        private void MoveToCurrentWaypoint()
        {
            if (_enemy.PatrolRoute == null || _enemy.PatrolRoute.WaypointCount == 0) return;
            Transform wp = _enemy.PatrolRoute.GetWaypoint(_waypointIndex);
            if (wp != null) _enemy.Agent.SetDestination(wp.position);
        }

        public void FixedTick() { }
        public void Exit() { }
    }
}