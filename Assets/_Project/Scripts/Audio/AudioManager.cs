using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SubjectZero.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private int sfxPoolSize = 12;

        private readonly List<AudioSource> _sfxPool = new();
        private readonly Dictionary<string, AudioSource> _loopChannels = new();
        private readonly Dictionary<string, Coroutine> _activeFades = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            BuildSfxPool();
        }

        private void BuildSfxPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var go = new GameObject($"SFX_{i}");
                go.transform.SetParent(transform);
                var src = go.AddComponent<AudioSource>();
                src.outputAudioMixerGroup = sfxGroup;
                src.playOnAwake = false;
                _sfxPool.Add(src);
            }
        }

        public void PlaySfx3D(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return;
            var src = GetFreeSfxSource();
            src.transform.position = position;
            src.spatialBlend = 1f;
            src.clip = clip;
            src.volume = volume;
            src.Play();
        }

        public void PlaySfx2D(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            var src = GetFreeSfxSource();
            src.spatialBlend = 0f;
            src.clip = clip;
            src.volume = volume;
            src.Play();
        }

        private AudioSource GetFreeSfxSource()
        {
            foreach (var src in _sfxPool)
                if (!src.isPlaying) return src;
            return _sfxPool[0]; // pool exhausted - steal the oldest rather than drop the sound
        }

        /// <summary>
        /// Starts (or crossfades into) a looping sound on a named channel - e.g.
        /// "entity_tension", "zone_ambience". Each channel owns exactly one
        /// AudioSource and one fade coroutine, so overlapping fades on different
        /// channels never interrupt each other.
        /// </summary>
        public void PlayLoop(string channel, AudioClip clip, AudioMixerGroup group, float targetVolume = 1f, float fadeTime = 1f)
        {
            if (!_loopChannels.TryGetValue(channel, out var src))
            {
                var go = new GameObject($"Loop_{channel}");
                go.transform.SetParent(transform);
                src = go.AddComponent<AudioSource>();
                src.loop = true;
                src.playOnAwake = false;
                _loopChannels[channel] = src;
            }

            src.outputAudioMixerGroup = group;
            if (src.clip != clip)
            {
                src.clip = clip;
                src.volume = 0f;
                src.Play();
            }

            RestartFade(channel, src, targetVolume, fadeTime);
        }

        public void StopLoop(string channel, float fadeTime = 1f)
        {
            if (_loopChannels.TryGetValue(channel, out var src))
                RestartFade(channel, src, 0f, fadeTime, stopOnComplete: true);
        }

        private void RestartFade(string channel, AudioSource src, float target, float duration, bool stopOnComplete = false)
        {
            if (_activeFades.TryGetValue(channel, out var running) && running != null)
                StopCoroutine(running);

            _activeFades[channel] = StartCoroutine(FadeRoutine(src, target, duration, stopOnComplete));
        }

        private IEnumerator FadeRoutine(AudioSource src, float target, float duration, bool stopOnComplete)
        {
            float start = src.volume;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime; // fades keep working even while paused
                src.volume = Mathf.Lerp(start, target, duration > 0f ? t / duration : 1f);
                yield return null;
            }
            src.volume = target;
            if (stopOnComplete) src.Stop();
        }

        /// <summary>Converts a 0-1 slider value to dB and sets it on an exposed mixer parameter - the hook the Settings menu will call.</summary>
        public void SetMixerVolume01(string exposedParam, float normalized01)
        {
            float dB = normalized01 > 0.0001f ? Mathf.Log10(normalized01) * 20f : -80f;
            mixer.SetFloat(exposedParam, dB);
        }

        /// <summary>Fades an existing loop channel to a new volume without touching its clip - used for cutscene audio ducking.</summary>
        public void SetLoopVolume(string channel, float targetVolume, float fadeTime)
        {
            if (_loopChannels.TryGetValue(channel, out var src))
                RestartFade(channel, src, targetVolume, fadeTime);
        }
    }
}