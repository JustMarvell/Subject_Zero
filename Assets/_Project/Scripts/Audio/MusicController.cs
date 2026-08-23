using UnityEngine;
using UnityEngine.Audio;
using SubjectZero.Core;
using SubjectZero.Character.Enemy;
using SubjectZero.World;

namespace SubjectZero.Audio
{
    public class MusicController : MonoBehaviour
    {
        public static MusicController Instance { get; private set; }

        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioClip mainMenuTheme;
        [SerializeField] private AudioClip zone1Theme;
        [SerializeField] private AudioClip zone2PatrolTheme;
        [SerializeField] private AudioClip zone2AlertTheme;
        [SerializeField] private AudioClip zone2ChaseTheme;
        [SerializeField] private AudioClip zone2LostTheme;
        [SerializeField] private float sceneThemeFadeTime = 2f;
        [SerializeField] private float situationalFadeTime = 2.5f;

        private const string Channel = "music";

        private EnemyController _entity;
        private ZoneLightingController _lightingController;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void HandleMainMenuLoaded()
        {
            UnsubscribeFromZone();
            PlayMusic(mainMenuTheme, sceneThemeFadeTime);
        }

        /// <summary>Called by GameManager right after a zone finishes loading.</summary>
        public void HandleZoneLoaded(string zoneSceneName, EnemyController entity)
        {
            UnsubscribeFromZone();
            _entity = entity;

            if (_entity != null)
                _entity.StateMachine.OnStateChanged += HandleEntityStateChanged;

            _lightingController = FindFirstObjectByType<ZoneLightingController>();
            if (_lightingController != null)
                _lightingController.OnBlackoutChanged += HandleBlackoutChanged;

            AudioClip theme = zoneSceneName.Contains("Zone1") ? zone1Theme : zone2PatrolTheme;
            PlayMusic(theme, sceneThemeFadeTime);
        }

        private void UnsubscribeFromZone()
        {
            if (_entity != null) _entity.StateMachine.OnStateChanged -= HandleEntityStateChanged;
            if (_lightingController != null) _lightingController.OnBlackoutChanged -= HandleBlackoutChanged;
            _entity = null;
            _lightingController = null;
        }

        private void HandleEntityStateChanged(IState newState)
        {
            if (_entity == null) return;

            if (newState == _entity.AlertState) PlayMusic(zone2AlertTheme, 0f);
            else if (newState == _entity.ChaseState) PlayMusic(zone2ChaseTheme, 0f);
            else if (newState == _entity.LostState) PlayMusic(zone2LostTheme, situationalFadeTime);
            else if (newState == _entity.PatrolState)
            {
                // Don't resolve back to the calm theme just because the entity gave
                // up - if the room is still dark, stay tense until the lights
                // actually come back (handled by HandleBlackoutChanged below).
                bool stillDark = _lightingController != null && _lightingController.IsBlackedOut;
                if (!stillDark) PlayMusic(zone2PatrolTheme, situationalFadeTime);
            }
        }

        private void HandleBlackoutChanged(bool restored)
        {
            if (restored) PlayMusic(zone2PatrolTheme, situationalFadeTime);
        }

        private void PlayMusic(AudioClip clip, float fadeTime)
        {
            if (clip == null) return;
            AudioManager.Instance.PlayLoop(Channel, clip, musicGroup, 1f, fadeTime);
        }
    }
}