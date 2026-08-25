using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SubjectZero.Core;
using System.Collections.Generic;

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
        [SerializeField] private TMP_Dropdown vsyncDropdown;
        [SerializeField] private TMP_Dropdown frameRateDropdown;

        private readonly int[] _frameRateOptions = { 30, 45, 60, 120, -1 };

        private bool _initializing;

        private void Awake()
        {
            // Populated dynamically so this can never drift out of sync with
            // however many Quality Levels the project actually defines.
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));

            vsyncDropdown.ClearOptions();
            vsyncDropdown.AddOptions(new List<string> { "Off", "On", "Adaptive" });

            frameRateDropdown.ClearOptions();
            frameRateDropdown.AddOptions(new List<string> { "30 FPS", "45 FPS", "60 FPS", "120 FPS", "Unlimited" });
        }

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
            vsyncDropdown.value = (int)s.CurrentVSyncMode;
            int fpsIndex = System.Array.IndexOf(_frameRateOptions, s.TargetFrameRate);
            frameRateDropdown.value = fpsIndex >= 0 ? fpsIndex : 4;

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
        public void OnVSyncChanged(int index) { if (!_initializing) SettingsManager.Instance.SetVSyncMode((SettingsManager.VSyncMode)index); }
        public void OnFrameRateChanged(int index) { if (!_initializing) SettingsManager.Instance.SetTargetFrameRate(_frameRateOptions[index]); }

        public void Close()
        {
            PlayerPrefs.Save();
            gameObject.SetActive(false);
        }
    }
}