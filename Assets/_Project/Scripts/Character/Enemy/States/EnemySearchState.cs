using SubjectZero.Core;
using UnityEngine;
using UnityEngine.AI;

namespace SubjectZero.Character.Enemy
{
    public class EnemySearchState : IState
    {
        private readonly EnemyController _enemy;
        private float _timer;
        private Vector3 _searchCenter;

        public EnemySearchState(EnemyController enemy) => _enemy = enemy;

        public void Enter()
        {
            _enemy.Agent.speed = _enemy.Config.searchSpeed;
            _timer = 0f;
            _searchCenter = _enemy.LastKnownPlayerPosition;
            PickRandomSearchPoint();
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

            if (_enemy.Perception.CanHearPlayer())
            {
                _enemy.LastKnownPlayerPosition = _enemy.PlayerTransform.position;
                _enemy.Agent.SetDestination(_enemy.LastKnownPlayerPosition);
            }
            else if (!_enemy.Agent.pathPending && _enemy.Agent.remainingDistance < 0.5f)
            {
                PickRandomSearchPoint();
            }

            if (_timer >= _enemy.Config.searchDuration)
                _enemy.StateMachine.ChangeState(_enemy.LostState);
        }

        private void PickRandomSearchPoint()
        {
            Vector2 offset = Random.insideUnitCircle * _enemy.Config.searchPointRadius;
            Vector3 candidate = _searchCenter + new Vector3(offset.x, 0f, offset.y);

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _enemy.Config.searchPointRadius, NavMesh.AllAreas))
                _enemy.Agent.SetDestination(hit.position);
        }

        public void FixedTick() { }
        public void Exit() { }
    }
}