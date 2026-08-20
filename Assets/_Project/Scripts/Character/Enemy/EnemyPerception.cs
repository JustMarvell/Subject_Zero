using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    public class EnemyPerception
    {
        private readonly EnemyController _enemy;

        public EnemyPerception(EnemyController enemy) => _enemy = enemy;

        public bool CanSeePlayer()
        {
            if (_enemy.Player.IsHidden) return false;

            var config = _enemy.Config;
            Transform player = _enemy.PlayerTransform;

            Vector3 eyePos = _enemy.transform.position + Vector3.up * config.eyeHeight;
            Vector3 toPlayer = player.position - eyePos;
            float distance = toPlayer.magnitude;

            if (distance > config.visionRange) return false;

            Vector3 dirToPlayer = toPlayer.normalized;
            float angle = Vector3.Angle(_enemy.transform.forward, dirToPlayer);
            if (angle > config.visionAngle * 0.5f) return false;

            if (Physics.Raycast(eyePos, dirToPlayer, out RaycastHit hit, distance,
                    config.visionBlockingMask, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag("Player"))
                    return false;
            }

            return true;
        }

        public bool CanHearPlayer()
        {
            if (_enemy.Player.IsHidden) return false;

            float playerNoise01 = _enemy.Player.CurrentNoiseLevel01;
            if (playerNoise01 <= 0f) return false;

            float distance = Vector3.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            float effectiveHearingRange = _enemy.Config.hearingRangeMax * playerNoise01;
            return distance <= effectiveHearingRange;
        }
    }
}