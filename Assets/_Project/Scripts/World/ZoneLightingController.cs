using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SubjectZero.Core;
using SubjectZero.Telemetry;

namespace SubjectZero.World
{
    public class ZoneLightingController : MonoBehaviour
    {
        [SerializeField] private List<Light> roomLights = new();
        [SerializeField] private Light directionalLight;
        [SerializeField] private float directionalDimIntensity = 0.05f;
        [SerializeField] private float directionalNormalIntensity = 0.2f;

        [Header("Flicker")]
        [SerializeField] private float flickerDuration = 0.6f;
        [SerializeField] private int flickerSteps = 6;

        [Header("Blackout timing")]
        [SerializeField] private float minRestoreDelay = 10f;
        [SerializeField] private float maxRestoreDelay = 20f;

        public bool IsBlackedOut { get; private set; }

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
                // Cancel any pending "restore lights" countdown - re-engaging means
                // the darkness should persist, not expire on the old timer.
                if (_activeRoutine != null) StopCoroutine(_activeRoutine);
                _activeRoutine = IsBlackedOut ? null : StartCoroutine(BlackoutRoutine());
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
            yield return Flicker(turningOn: false);
            SetLights(false);
            IsBlackedOut = true;
            TelemetryManager.Instance?.RecordDarknessStart();
        }

        private IEnumerator RestoreAfterDelayRoutine(float delay)
        {
            float t = 0f;
            while (t < delay)
            {
                t += Time.deltaTime;
                yield return null;
            }

            yield return Flicker(turningOn: true);
            SetLights(true);
            IsBlackedOut = false;
            TelemetryManager.Instance?.RecordDarknessEnd();
        }

        private IEnumerator Flicker(bool turningOn)
        {
            float stepDuration = flickerDuration / flickerSteps;
            for (int i = 0; i < flickerSteps; i++)
            {
                bool on = turningOn ? (i % 2 == 0) : (i % 2 != 0);
                SetLights(on, includeDirectional: false); // dim fill light stays steady, only room lights flicker
                yield return new WaitForSeconds(stepDuration);
            }
        }

        private void SetLights(bool on, bool includeDirectional = true)
        {
            foreach (var light in roomLights)
                if (light != null) light.enabled = on;

            if (includeDirectional && directionalLight != null)
                directionalLight.intensity = on ? directionalNormalIntensity : directionalDimIntensity;
        }
    }
}