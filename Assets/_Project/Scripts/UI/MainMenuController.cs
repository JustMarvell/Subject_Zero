using UnityEngine;
using SubjectZero.Core;

namespace SubjectZero.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject settingsPanel; // stub - full settings menu comes in a later pass

        public void OnPlayButton()
        {
            LoadingScreenController.Instance.LoadSceneSingle("Bootstrap", () =>
            {
                GameManager.Instance.LoadFirstZone(null);
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