using UnityEngine;
using SubjectZero.Core;
using SubjectZero.Audio;
using SubjectZero.Cutscene;

namespace SubjectZero.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel;

        private void Start() => MusicController.Instance?.HandleMainMenuLoaded();

        public void OnPlayButton()
        {
            LoadingScreenController.Instance.LoadSceneSingle("Bootstrap", () =>
            {
                GameManager.Instance.LoadFirstZone(() => IntroCutsceneController.Instance.Play());
            });
        }

        public void OnSettingsButton()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        public void OnQuitButton()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}