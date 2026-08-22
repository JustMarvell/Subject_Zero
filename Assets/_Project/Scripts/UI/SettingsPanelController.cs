using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SubjectZero.Core;

namespace SubjectZero.UI
{
    public class SettingsPanelController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider ambienceSlider;

        [Header("Controls")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private Toggle invertYToggle;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        private bool _initializing;

        private void OnEnable()
        {
            _initializing = true;
            var s = SettingsManager.Instance;

            masterSlider.value = s.MasterVolume01;
            musicSlider.value = s.MusicVolume01;
            sfxSlider.value = s.SFXVolume01;
            ambienceSlider.value = s.AmbienceVolume01;
            sensitivitySlider.value = s.SensitivityMultiplier;
            invertYToggle.isOn = s.InvertY;
            qualityDropdown.value = s.QualityLevel;
            fullscreenToggle.isOn = s.Fullscreen;

            _initializing = false;
        }

        // Wire each to the matching UI element's OnValueChanged in the Inspector
        public void OnMasterVolumeChanged(float v) { if (!_initializing) SettingsManager.Instance.SetMasterVolume(v); }
        public void OnMusicVolumeChanged(float v) { if (!_initializing) SettingsManager.Instance.SetMusicVolume(v); }
        public void OnSFXVolumeChanged(float v) { if (!_initializing) SettingsManager.Instance.SetSFXVolume(v); }
        public void OnAmbienceVolumeChanged(float v) { if (!_initializing) SettingsManager.Instance.SetAmbienceVolume(v); }
        public void OnSensitivityChanged(float v) { if (!_initializing) SettingsManager.Instance.SetSensitivity(v); }
        public void OnInvertYChanged(bool v) { if (!_initializing) SettingsManager.Instance.SetInvertY(v); }
        public void OnQualityChanged(int v) { if (!_initializing) SettingsManager.Instance.SetQualityLevel(v); }
        public void OnFullscreenChanged(bool v) { if (!_initializing) SettingsManager.Instance.SetFullscreen(v); }

        public void Close()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}