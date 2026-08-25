using UnityEngine;

namespace SubjectZero.Core
{
    /// <summary>
    /// Unity has no native adaptive VSync toggle. This approximates it: vsync
    /// stays on while the framerate comfortably keeps up with the display's
    /// refresh rate, and switches off (allowing tearing) the moment it drops
    /// below, to avoid the stutter a strict VSync-On would cause otherwise.
    /// </summary>
    public class AdaptiveVSyncController : MonoBehaviour
    {
        [SerializeField] private float checkInterval = 0.5f;
        [SerializeField] private float dropThreshold = 0.95f;

        private float _timer;

        private void Update()
        {
            if (SettingsManager.Instance == null ||
                SettingsManager.Instance.CurrentVSyncMode != SettingsManager.VSyncMode.Adaptive)
                return;

            _timer += Time.unscaledDeltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            float targetRefresh = (float)Screen.currentResolution.refreshRateRatio.value;
            if (targetRefresh <= 0f) targetRefresh = 60f;

            float currentFps = 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            QualitySettings.vSyncCount = currentFps >= targetRefresh * dropThreshold ? 1 : 0;
        }
    }
}