using UnityEngine;
using SubjectZero.Input;
using SubjectZero.Telemetry;
using SubjectZero.UI;
using SubjectZero.Interaction.Examples;

namespace SubjectZero.Core
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private GameObject pauseCanvasRoot;
        [SerializeField] private GameObject settingsPanel;

        public bool IsPaused { get; private set; }

        private void Start() => pauseCanvasRoot.SetActive(false);

        private void Update()
        {
            if (!inputReader.PausePressedThisFrame || IsPaused) return;
            if (CaughtSequenceController.Instance != null && CaughtSequenceController.Instance.IsActive) return;
            if (GameCompleteController.Instance != null && GameCompleteController.Instance.IsActive) return;
            if (TextDocumentController.Instance != null && TextDocumentController.Instance.IsReading) return;

            SetPaused(true);
        }

        public void TogglePause() => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            AudioListener.pause = paused;
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            pauseCanvasRoot.SetActive(paused);
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        public void OnResumeButton() => SetPaused(false);
        public void OnSettingsButton() => settingsPanel.SetActive(true);

        public void OnQuitToMainMenuButton()
        {
            Time.timeScale = 1f;
            TelemetryUploader.Instance?.UploadCurrentSession();
            LoadingScreenController.Instance.LoadSceneSingle("MainMenu");
        }
    }
}