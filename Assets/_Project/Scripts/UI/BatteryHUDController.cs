using UnityEngine;
using UnityEngine.UI;
using SubjectZero.Character.Player;

namespace SubjectZero.UI
{
    public class BatteryHUDController : MonoBehaviour
    {
        [SerializeField] private FlashlightController flashlight;
        [SerializeField] private Image fillImage;

        private void Update()
        {
            if (flashlight == null || fillImage == null) return;
            fillImage.fillAmount = flashlight.BatteryPercent01;
        }
    }
}