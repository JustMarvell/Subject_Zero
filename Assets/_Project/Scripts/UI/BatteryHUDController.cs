using UnityEngine;
using UnityEngine.UI;
using SubjectZero.Character.Player;
using SubjectZero.Telemetry;

namespace SubjectZero.UI
{
    public class BatteryHUDController : MonoBehaviour
    {
        [SerializeField] private FlashlightController flashlight;
        [SerializeField] private Image fillImage;
        [SerializeField] private GameObject visualsRoot;

        private bool _lastVisible = true;

        private void Update()
        {
            if (flashlight == null || fillImage == null) return;

            bool visible = TelemetryManager.Instance != null && TelemetryManager.Instance.FlashlightRelevant;
            if (visible != _lastVisible)
            {
                _lastVisible = visible;
                if (visualsRoot != null) visualsRoot.SetActive(visible);
            }

            if (visible)
                fillImage.fillAmount = flashlight.BatteryPercent01;
        }
    }
}