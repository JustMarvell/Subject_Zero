using System.Collections;
using UnityEngine;
using SubjectZero.Core;
using SubjectZero.Telemetry;

namespace SubjectZero.World
{
    public class ZoneLightingController : MonoBehaviour
    {
        [SerializeField] private FlickeringLightGroup lightGroup;
        [SerializeField] private float minRestoreDelay = 10f;
        [SerializeField] private float maxRestoreDelay = 20f;

        public bool IsBlackedOut => !lightGroup.IsOn;
        public event System.Action<bool> OnBlackoutChanged;

        private Coroutine _activeRoutine;

        private void Start()
        {
            GameManager.Instance.Entity.StateMachine.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null && GameManager.Instance.Entity != null)
                GameManager.Instance.Entity.StateMachine.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(IState newState)
        {
            var entity = GameManager.Instance.Entity;
            bool isThreatState = newState == entity.AlertState || newState == entity.ChaseState;

            if (isThreatState)
            {
                if (_activeRoutine != null) StopCoroutine(_activeRoutine);
                _activeRoutine = lightGroup.IsOn ? StartCoroutine(BlackoutRoutine()) : null;
            }
            else if (newState == entity.LostState)
            {
                float delay = Random.Range(minRestoreDelay, maxRestoreDelay);
                if (_activeRoutine != null) StopCoroutine(_activeRoutine);
                _activeRoutine = StartCoroutine(RestoreAfterDelayRoutine(delay));
            }
        }

        private IEnumerator BlackoutRoutine()
        {
            yield return lightGroup.PlayFlicker(false);
            TelemetryManager.Instance?.RecordDarknessStart();
            OnBlackoutChanged?.Invoke(false);
        }

        private IEnumerator RestoreAfterDelayRoutine(float delay)
        {
            float t = 0f;
            while (t < delay) { t += Time.deltaTime; yield return null; }

            yield return lightGroup.PlayFlicker(true);
            TelemetryManager.Instance?.RecordDarknessEnd();
            OnBlackoutChanged?.Invoke(true);
        }
    }
}