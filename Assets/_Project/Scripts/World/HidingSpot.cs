using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.Telemetry;

namespace SubjectZero.World
{
    /// <summary>
    /// Trigger volume representing a hiding spot (behind a crate, under a desk, etc).
    /// Sets PlayerController.IsHidden, which EnemyPerception checks to suppress
    /// detection, and fires the hide-start/end telemetry hooks.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HidingSpot : MonoBehaviour
    {
        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            player.IsHidden = true;
            TelemetryManager.Instance?.RecordHideStart();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var player = other.GetComponent<PlayerController>();
            if (player == null) return;

            player.IsHidden = false;
            TelemetryManager.Instance?.RecordHideEnd();
        }
    }
}