using UnityEngine;
using SubjectZero.Input;
using SubjectZero.Telemetry;
using SubjectZero.Audio;

namespace SubjectZero.Character.Player
{
    public class FlashlightController : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private FlashlightConfig config;
        [SerializeField] private Light flashlightLight;
        [SerializeField] private AudioClip toggleFlashlightClip;

        private float _currentBattery;
        private bool _isOn;
        private float _flickerSeed;

        public float BatteryPercent01 => config.capacitySeconds > 0f
            ? Mathf.Clamp01(_currentBattery / config.capacitySeconds)
            : 0f;

        private void Awake()
        {
            _currentBattery = config.capacitySeconds;
            _flickerSeed = Random.value * 100f;
            flashlightLight.enabled = false;
            flashlightLight.intensity = config.lightIntensity;
        }

        private void Update()
        {
            if (inputReader.FlashlightPressedThisFrame)
                TryToggle();

            if (_isOn)
            {
                _currentBattery -= Time.deltaTime;
                if (_currentBattery <= 0f)
                {
                    _currentBattery = 0f;
                    SetOn(false);
                }
            }

            player.IsFlashlightOn = _isOn;
            UpdateLightVisual();
        }

        private void TryToggle()
        {
            AudioManager.Instance.PlaySfx3D(toggleFlashlightClip, transform.position);
            if (_isOn) { SetOn(false); return; }
            if (_currentBattery > 0f) SetOn(true);
        }

        private void SetOn(bool on)
        {
            _isOn = on;
            flashlightLight.enabled = on;
        }

        private void UpdateLightVisual()
        {
            if (!_isOn) return;

            if (BatteryPercent01 <= config.lowBatteryThreshold)
            {
                float noise = Mathf.PerlinNoise(_flickerSeed, Time.time * 8f);
                flashlightLight.intensity = Mathf.Max(0f, config.lightIntensity * (1f - config.flickerAmount * noise));
            }
            else
            {
                flashlightLight.intensity = config.lightIntensity;
            }
        }

        public void AddBattery(float seconds)
        {
            _currentBattery = Mathf.Min(_currentBattery + seconds, config.capacitySeconds);
        }
    }
}