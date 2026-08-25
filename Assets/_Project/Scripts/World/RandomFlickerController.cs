using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SubjectZero.Telemetry;

namespace SubjectZero.World
{
    public class RandomFlickerController : MonoBehaviour
    {
        [System.Serializable]
        public class RoomLightEntry
        {
            public string roomName; // Inspector clarity only
            public FlickeringLightGroup group;
        }

        [SerializeField] private List<RoomLightEntry> rooms = new();

        [Header("Ambient (background) triggering")]
        [SerializeField] private float ambientCheckInterval = 20f;
        [SerializeField] private float ambientTriggerChance01 = 0.35f;

        [Header("Room-entry triggering (additive on top of ambient)")]
        [SerializeField] private float roomEntryTriggerChance01 = 0.2f;

        [Header("Event type mix")]
        [Tooltip("Chance a triggered event is just a brief flicker (stays lit) instead of a full blackout.")]
        [SerializeField] private float flickerOnlyChance01 = 0.5f;

        [Header("Blackout restore timing (flicker-to-off events only)")]
        [SerializeField] private float minRestoreDelay = 8f;
        [SerializeField] private float maxRestoreDelay = 15f;

        private readonly Dictionary<FlickeringLightGroup, Coroutine> _activeRoutines = new();
        private float _ambientTimer;

        private void Update()
        {
            _ambientTimer += Time.deltaTime;
            if (_ambientTimer < ambientCheckInterval) return;

            _ambientTimer = 0f;
            if (rooms.Count == 0 || Random.value > ambientTriggerChance01) return;

            var entry = rooms[Random.Range(0, rooms.Count)];
            TryTriggerEvent(entry.group);
        }

        /// <summary>Called by RoomEntryFlickerTrigger when the player enters a room.</summary>
        public void TryRoomEntryEvent(FlickeringLightGroup group)
        {
            if (Random.value <= roomEntryTriggerChance01)
                TryTriggerEvent(group);
        }

        private void TryTriggerEvent(FlickeringLightGroup group)
        {
            if (group == null || _activeRoutines.ContainsKey(group)) return; // already mid-event

            bool flickerOnly = Random.value <= flickerOnlyChance01;
            _activeRoutines[group] = StartCoroutine(RunEvent(group, flickerOnly));
        }

        private IEnumerator RunEvent(FlickeringLightGroup group, bool flickerOnly)
        {
            if (flickerOnly)
            {
                yield return group.PlayFlicker(true);
            }
            else
            {
                yield return group.PlayFlicker(false);
                TelemetryManager.Instance?.RecordDarknessStart();

                float delay = Random.Range(minRestoreDelay, maxRestoreDelay);
                float t = 0f;
                while (t < delay) { t += Time.deltaTime; yield return null; }

                yield return group.PlayFlicker(true);
                TelemetryManager.Instance?.RecordDarknessEnd();
            }

            _activeRoutines.Remove(group);
        }
    }
}