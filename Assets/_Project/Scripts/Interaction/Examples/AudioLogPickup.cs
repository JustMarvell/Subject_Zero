using System.Collections;
using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.UI;
using SubjectZero.Story;

namespace SubjectZero.Interaction.Examples
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioLogPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private AudioLogData data;
        [SerializeField] private float fadeOutDuration = 2f;
        [SerializeField] private float pitchDropTarget = 0.5f;

        public static AudioLogPickup CurrentlyPlaying { get; private set; }

        private AudioSource _audioSource;
        private PlayerController _player;
        private Coroutine _subtitleRoutine;
        private bool _isPlaying;

        public string InteractionPrompt => _isPlaying ? $"Stop '{data.logTitle}'" : $"Play '{data.logTitle}'";
        public bool CanInteract(PlayerController player) => data != null && data.clip != null;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = data.clip;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 1f;
        }

        public void Interact(PlayerController player)
        {
            _player = player;
            if (_isPlaying) StopImmediate();
            else StartPlayback();
        }

        private void StartPlayback()
        {
            _audioSource.pitch = 1f;
            _audioSource.volume = 1f;
            _audioSource.Stop();
            _audioSource.Play();

            _isPlaying = true;
            _player.IsPlayingAudioLog = true;
            CurrentlyPlaying = this;

            SubtitleUIController.Instance?.Show();
            _subtitleRoutine = StartCoroutine(SubtitleRoutine());
            StartCoroutine(WatchForNaturalEnd());
        }

        private void StopImmediate()
        {
            _audioSource.Stop();
            EndPlaybackCommon();
        }

        private IEnumerator WatchForNaturalEnd()
        {
            while (_isPlaying)
            {
                if (!AudioListener.pause && !_audioSource.isPlaying)
                    break;

                yield return null;
            }

            if (_isPlaying) EndPlaybackCommon();
        }

        private void EndPlaybackCommon()
        {
            _isPlaying = false;
            if (_player != null) _player.IsPlayingAudioLog = false;
            if (CurrentlyPlaying == this) CurrentlyPlaying = null;

            if (_subtitleRoutine != null) { StopCoroutine(_subtitleRoutine); _subtitleRoutine = null; }
            SubtitleUIController.Instance?.Hide();
        }

        private IEnumerator SubtitleRoutine()
        {
            int index = 0;
            while (_isPlaying && index < data.subtitles.Length)
            {
                if (_audioSource.time >= data.subtitles[index].startTime)
                {
                    SubtitleUIController.Instance?.SetText(data.subtitles[index].text);
                    index++;
                }
                yield return null;
            }
        }

        /// <summary>Called by CaughtSequenceController/GameCompleteController - subtitle
        /// disappears immediately, audio fades and drops pitch for a bit of drama.</summary>
        public void HandlePlayerCaughtFade()
        {
            if (!_isPlaying) return;

            _isPlaying = false;
            if (_player != null) _player.IsPlayingAudioLog = false;
            if (CurrentlyPlaying == this) CurrentlyPlaying = null;

            if (_subtitleRoutine != null) { StopCoroutine(_subtitleRoutine); _subtitleRoutine = null; }
            SubtitleUIController.Instance?.Hide();

            StartCoroutine(FadeOutRoutine());
        }

        private IEnumerator FadeOutRoutine()
        {
            float startVolume = _audioSource.volume;
            float startPitch = _audioSource.pitch;
            float t = 0f;

            while (t < fadeOutDuration)
            {
                t += Time.unscaledDeltaTime;
                float f = t / fadeOutDuration;
                _audioSource.volume = Mathf.Lerp(startVolume, 0f, f);
                _audioSource.pitch = Mathf.Lerp(startPitch, pitchDropTarget, f);
                yield return null;
            }

            _audioSource.Stop();
            _audioSource.pitch = 1f;
        }
    }
}