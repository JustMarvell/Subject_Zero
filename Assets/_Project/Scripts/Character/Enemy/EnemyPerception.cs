using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    /// <summary>
    /// Vision cone + hearing checks. Plain class (not a state) since every state
    /// needs to query perception, same pattern as PlayerStance being shared
    /// across player states.
    /// </summary>
    public class EnemyPerception
    {
        private readonly EnemyController _enemy;

        public EnemyPerception(EnemyController enemy) => _enemy = enemy;

        public bool CanSeePlayer()
        {
            var config = _enemy.Config;
            Transform player = _enemy.PlayerTransform;

            Vector3 eyePos = _enemy.transform.position + Vector3.up * config.eyeHeight;
            Vector3 toPlayer = player.position - eyePos;
            float distance = toPlayer.magnitude;

            if (distance > config.visionRange) return false;

            Vector3 dirToPlayer = toPlayer.normalized;
            float angle = Vector3.Angle(_enemy.transform.forward, dirToPlayer);
            if (angle > config.visionAngle * 0.5f) return false;

            // If something is hit before reaching the player and it's not the
            // player itself, line of sight is blocked.
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
            float playerNoise01 = _enemy.Player.CurrentNoiseLevel01;
            if (playerNoise01 <= 0f) return false;

            float distance = Vector3.Distance(_enemy.transform.position, _enemy.PlayerTransform.position);
            float effectiveHearingRange = _enemy.Config.hearingRangeMax * playerNoise01;
            return distance <= effectiveHearingRange;
        }
    }
}