using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SubjectZero.Core
{
    /// <summary>
    /// Single authority for all scene loading - both the initial MainMenu -> Bootstrap
    /// transition and every zone-to-zone transition route through here, rather than
    /// each caller invoking SceneManager independently.
    /// </summary>
    public class LoadingScreenController : MonoBehaviour
    {
        public static LoadingScreenController Instance { get; private set; }

        [SerializeField] private GameObject loadingCanvasRoot;
        [SerializeField] private Slider progressBar;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            loadingCanvasRoot.SetActive(false);
        }

        /// <summary>Replaces everything currently loaded - used for MainMenu -> Bootstrap.</summary>
        public void LoadSceneSingle(string sceneName, Action onComplete = null)
        {
            StartCoroutine(LoadRoutine(sceneName, LoadSceneMode.Single, null, onComplete));
        }

        /// <summary>Unloads oldSceneName (if any) and additively loads newSceneName - used for zone transitions.</summary>
        public void LoadSceneAdditive(string newSceneName, string unloadSceneName, Action onComplete = null)
        {
            StartCoroutine(LoadRoutine(newSceneName, LoadSceneMode.Additive, unloadSceneName, onComplete));
        }

        private IEnumerator LoadRoutine(string sceneName, LoadSceneMode mode, string unloadSceneName, Action onComplete)
        {
            loadingCanvasRoot.SetActive(true);
            if (progressBar != null) progressBar.value = 0f;

            if (!string.IsNullOrEmpty(unloadSceneName))
                yield return SceneManager.UnloadSceneAsync(unloadSceneName);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, mode);

            while (!op.isDone)
            {
                if (progressBar != null) progressBar.value = op.progress;
                yield return null;
            }

            if (mode == LoadSceneMode.Additive)
            {
                Scene loaded = SceneManager.GetSceneByName(sceneName);
                if (loaded.IsValid() && loaded.isLoaded)
                    SceneManager.SetActiveScene(loaded);
            }

            loadingCanvasRoot.SetActive(false);
            onComplete?.Invoke();
        }
    }
}