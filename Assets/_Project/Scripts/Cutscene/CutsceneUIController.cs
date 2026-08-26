using System.Collections;
using TMPro;
using UnityEngine;
using SubjectZero.Audio;

namespace SubjectZero.Cutscene
{
    public class CutsceneUIController : MonoBehaviour
    {
        public static CutsceneUIController Instance { get; private set; }

        [SerializeField] private CanvasGroup blackOverlay;
        [SerializeField] private TMP_Text typedText;
        [SerializeField] private AudioClip typingSfx;

        [Header("Audio ducking during cutscenes")]
        [SerializeField] private float duckedMusicVolume = 0.15f;
        [SerializeField] private float duckedEntityVolume = 0f;
        [SerializeField] private float duckFadeTime = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            SetBlackInstant(false);
            ClearText();
        }

        public void SetBlackInstant(bool black)
        {
            blackOverlay.alpha = black ? 1f : 0f;
            blackOverlay.blocksRaycasts = black;
        }

        public IEnumerator FadeToBlack(float duration)
        {
            yield return FadeOverlay(1f, duration);
            blackOverlay.blocksRaycasts = true;
        }

        public IEnumerator FadeFromBlack(float duration)
        {
            blackOverlay.blocksRaycasts = false;
            yield return FadeOverlay(0f, duration);
        }

        private IEnumerator FadeOverlay(float target, float duration)
        {
            float start = blackOverlay.alpha;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                blackOverlay.alpha = Mathf.Lerp(start, target, duration > 0f ? t / duration : 1f);
                yield return null;
            }
            blackOverlay.alpha = target;
        }

        public IEnumerator TypeText(string fullText, float charsPerSecond)
        {
            typedText.text = "";
            AudioSource typingLoop = typingSfx != null ? StartTypingSfx() : null;

            float delay = 1f / Mathf.Max(1f, charsPerSecond);
            foreach (char c in fullText)
            {
                typedText.text += c;
                yield return new WaitForSecondsRealtime(delay);
            }

            if (typingLoop != null) typingLoop.Stop();
        }

        private AudioSource StartTypingSfx()
        {
            var go = new GameObject("TypingSfxSource");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.clip = typingSfx;
            src.loop = true;
            src.Play();
            return src;
        }

        public void ClearText()
        {
            if (typedText != null) typedText.text = "";
        }

        public void DuckGameplayAudio()
        {
            AudioManager.Instance.SetLoopVolume("music", duckedMusicVolume, duckFadeTime);
            AudioManager.Instance.SetLoopVolume("ambience", duckedMusicVolume, duckFadeTime);
            AudioManager.Instance.SetLoopVolume("entity_tension", duckedEntityVolume, duckFadeTime);
        }

        public void RestoreGameplayAudio()
        {
            AudioManager.Instance.SetLoopVolume("music", 1f, duckFadeTime);
            AudioManager.Instance.SetLoopVolume("ambience", duckedMusicVolume, duckFadeTime);
            AudioManager.Instance.SetLoopVolume("entity_tension", 1f, duckFadeTime);
        }
    }
}