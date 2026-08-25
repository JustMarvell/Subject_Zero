using TMPro;
using UnityEngine;

namespace SubjectZero.UI
{
    public class SubtitleUIController : MonoBehaviour
    {
        public static SubtitleUIController Instance { get; private set; }

        [SerializeField] private GameObject subtitleRoot;
        [SerializeField] private TMP_Text subtitleText;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => subtitleRoot.SetActive(false);

        public void Show() => subtitleRoot.SetActive(true);
        public void Hide() => subtitleRoot.SetActive(false);
        public void SetText(string text) { if (subtitleText != null) subtitleText.text = text; }
    }
}