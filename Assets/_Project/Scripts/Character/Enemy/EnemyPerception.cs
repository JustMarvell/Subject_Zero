using UnityEngine;

namespace SubjectZero.Character.Enemy
{
    public class EnemyPerception
    {
        private readonly EnemyController _enemy;

        public EnemyPerception(EnemyController enemy) => _enemy = enemy;

        public bool CanSeePlayer()
        {
            // Flashlight gives away a hidden player's position.
            if (_enemy.Player.IsHidden && !_enemy.Player.IsFlashlightOn) return false;

            var config = _enemy.Config;
            Transform player = _enemy.PlayerTransform;

            Vector3 eyePos = _enemy.transform.position + Vector3.up * config.eyeHeight;
            Vector3 toPlayer = player.position - eyePos;
            float distance = toPlayer.magnitude;

            float effectiveVisionRange = _enemy.CurrentVisionRange;
            if (_enemy.Player.IsFlashlightOn)
                effectiveVisionRange *= config.flashlightVisionMultiplier;

            if (distance > effectiveVisionRange) return false;

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
            var player = _enemy.Player;
            float distance = Vector3.Distance(_enemy.transform.position, player.transform.position);

            // Deliberately bypasses the hidden-check below - playing a log is audible
            // even while hiding, unlike movement noise.
            if (player.IsPlayingAudioLog && distance <= _enemy.Config.audioLogHearingRange)
                return true;

            if (player.IsHidden) return false;

            float playerNoise01 = player.CurrentNoiseLevel01;
            if (playerNoise01 <= 0f) return false;

            float effectiveHearingRange = _enemy.Config.hearingRangeMax * playerNoise01;
            return distance <= effectiveHearingRange;
        }
    }
}