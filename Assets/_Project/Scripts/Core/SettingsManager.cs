using UnityEngine;
using SubjectZero.Audio;

namespace SubjectZero.Core
{
    /// <summary>
    /// Single source of truth for user-adjustable settings. Persists via PlayerPrefs.
    /// Never mutates ScriptableObject configs directly - other systems read the
    /// current values live from here instead.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public enum VSyncMode { Off, On, Adaptive }

        public static SettingsManager Instance { get; private set; }

        public VSyncMode CurrentVSyncMode { get; private set; } = VSyncMode.On;

        public float MasterVolume01 { get; private set; } = 1f;
        public float MusicVolume01 { get; private set; } = 0.8f;
        public float SFXVolume01 { get; private set; } = 1f;
        public float AmbienceVolume01 { get; private set; } = 0.8f;
        public float SensitivityMultiplier { get; private set; } = 1f;
        public bool InvertY { get; private set; }
        public int QualityLevel { get; private set; }
        public int TargetFrameRate { get; private set; } = -1; // -1 = unlimited
        public bool Fullscreen { get; private set; } = true;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            LoadSettings();
        }

        private void Start()
        {
            ApplyAudio();
            ApplyGraphics();
        }

        private void OnApplicationQuit() => PlayerPrefs.Save();

        private void LoadSettings()
        {
            MasterVolume01 = PlayerPrefs.GetFloat("Settings_MasterVolume", 1f);
            MusicVolume01 = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.8f);
            SFXVolume01 = PlayerPrefs.GetFloat("Settings_SFXVolume", 1f);
            AmbienceVolume01 = PlayerPrefs.GetFloat("Settings_AmbienceVolume", 0.8f);
            SensitivityMultiplier = PlayerPrefs.GetFloat("Settings_Sensitivity", 1f);
            InvertY = PlayerPrefs.GetInt("Settings_InvertY", 0) == 1;
            QualityLevel = PlayerPrefs.GetInt("Settings_QualityLevel", QualitySettings.GetQualityLevel());
            Fullscreen = PlayerPrefs.GetInt("Settings_Fullscreen", 1) == 1;
            CurrentVSyncMode = (VSyncMode)PlayerPrefs.GetInt("Settings_VSyncMode", (int)VSyncMode.On);
            TargetFrameRate = PlayerPrefs.GetInt("Settings_TargetFrameRate", -1);
        }

        public void SetMasterVolume(float v01)
        {
            MasterVolume01 = v01;
            AudioManager.Instance.SetMixerVolume01("MasterVolume", v01);
            PlayerPrefs.SetFloat("Settings_MasterVolume", v01);
        }

        public void SetMusicVolume(float v01)
        {
            MusicVolume01 = v01;
            AudioManager.Instance.SetMixerVolume01("MusicVolume", v01);
            PlayerPrefs.SetFloat("Settings_MusicVolume", v01);
        }

        public void SetSFXVolume(float v01)
        {
            SFXVolume01 = v01;
            AudioManager.Instance.SetMixerVolume01("SFXVolume", v01);
            PlayerPrefs.SetFloat("Settings_SFXVolume", v01);
        }

        public void SetAmbienceVolume(float v01)
        {
            AmbienceVolume01 = v01;
            AudioManager.Instance.SetMixerVolume01("AmbienceVolume", v01);
            PlayerPrefs.SetFloat("Settings_AmbienceVolume", v01);
        }

        public void SetSensitivity(float multiplier)
        {
            SensitivityMultiplier = multiplier;
            PlayerPrefs.SetFloat("Settings_Sensitivity", multiplier);
        }

        public void SetInvertY(bool invert)
        {
            InvertY = invert;
            PlayerPrefs.SetInt("Settings_InvertY", invert ? 1 : 0);
        }

        public void SetQualityLevel(int level)
        {
            QualityLevel = level;
            QualitySettings.SetQualityLevel(level, true);
            PlayerPrefs.SetInt("Settings_QualityLevel", level);
        }

        public void SetFullscreen(bool fullscreen)
        {
            Fullscreen = fullscreen;
            Screen.fullScreenMode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            PlayerPrefs.SetInt("Settings_Fullscreen", fullscreen ? 1 : 0);
        }

        private void ApplyAudio()
        {
            AudioManager.Instance.SetMixerVolume01("MasterVolume", MasterVolume01);
            AudioManager.Instance.SetMixerVolume01("MusicVolume", MusicVolume01);
            AudioManager.Instance.SetMixerVolume01("SFXVolume", SFXVolume01);
            AudioManager.Instance.SetMixerVolume01("AmbienceVolume", AmbienceVolume01);
        }

        private void ApplyGraphics()
        {
            QualitySettings.SetQualityLevel(QualityLevel, true);
            Screen.fullScreenMode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            ApplyVSync();
        }

        public void SetVSyncMode(VSyncMode mode)
        {
            CurrentVSyncMode = mode;
            PlayerPrefs.SetInt("Settings_VSyncMode", (int)mode);
            ApplyVSync();
        }

        public void SetTargetFrameRate(int fps)
        {
            TargetFrameRate = fps;
            PlayerPrefs.SetInt("Settings_TargetFrameRate", fps);
            ApplyFrameRate();
        }

        private void ApplyVSync()
        {
            switch (CurrentVSyncMode)
            {
                case VSyncMode.Off: QualitySettings.vSyncCount = 0; break;
                case VSyncMode.On: QualitySettings.vSyncCount = 1; break;
                case VSyncMode.Adaptive: QualitySettings.vSyncCount = 0; break; // AdaptiveVSyncController takes over from here
            }
            ApplyFrameRate();
        }

        private void ApplyFrameRate()
        {
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}