using UnityEngine;
using UnityEngine.Playables;
using SubjectZero.Audio;

namespace SubjectZero.Cutscene
{
    public class IntroCutsceneController : MonoBehaviour
    {
        public static IntroCutsceneController Instance { get; private set; }

        [SerializeField] private PlayableDirector director;

        [TextArea(5, 15)]
        [SerializeField]
        private string logText =
            "Log entry.\n\nMy brother hasn't answered in three weeks. " +
            "The last thing he sent me was a location - Halcyon. " +
            "I don't know what happened there. I'm going to find out.";
        [SerializeField] private float typingCharsPerSecond = 18f;
        [SerializeField] private float fadeFromBlackDuration = 2f;
        [SerializeField] private AudioClip lockOpenSfx;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>The cross-scene entry point - called by MainMenuController.
        /// Everything else below is called BY the Timeline itself via Signals,
        /// once playback has actually started.</summary>
        /// This is not used. IGNORE THISS
        public void Play() => director.Play();

        // Signal at the moment typing should begin
        public void PlayTypedText()
        {
            StartCoroutine(CutsceneUIController.Instance.TypeText(logText, typingCharsPerSecond));
        }

        // Signal at the moment the lock-opening sound should play
        public void PlayLockOpenSfx()
        {
            if (lockOpenSfx != null)
                AudioManager.Instance.PlaySfx2D(lockOpenSfx, 1f);
        }

        // Signal at the moment the fade-out should begin
        public void BeginFadeFromBlack()
        {
            StartCoroutine(CutsceneUIController.Instance.FadeFromBlack(fadeFromBlackDuration));
        }
    }
}