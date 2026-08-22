using UnityEngine;
using SubjectZero.Input;

namespace SubjectZero.Core
{
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private GameObject pauseCanvasRoot;

        public bool IsPaused { get; private set; }

        private void Start() => pauseCanvasRoot.SetActive(false);

        private void Update()
        {
            if (inputReader.PausePressedThisFrame)
                TogglePause();
        }

        public void TogglePause() => SetPaused(!IsPaused);

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            pauseCanvasRoot.SetActive(paused);
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        // Wire to the Resume button's OnClick in the Inspector
        public void OnResumeButton() => SetPaused(false);

        // Wire to the "Quit to Main Menu" button's OnClick in the Inspector
        public void OnQuitToMainMenuButton()
        {
            Time.timeScale = 1f;
            LoadingScreenController.Instance.LoadSceneSingle("MainMenu");
        }
    }
}