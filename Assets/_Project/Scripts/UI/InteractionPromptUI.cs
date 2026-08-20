using TMPro;
using UnityEngine;
using SubjectZero.Interaction;

namespace SubjectZero.UI
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private TextMeshProUGUI promptText;

        private void OnEnable() => interactor.OnFocusChanged += HandleFocusChanged;
        private void OnDisable() => interactor.OnFocusChanged -= HandleFocusChanged;

        private void HandleFocusChanged(IInteractable interactable)
        {
            if (interactable == null)
            {
                promptRoot.SetActive(false);
                return;
            }

            promptRoot.SetActive(true);
            promptText.text = $"[E] {interactable.InteractionPrompt}";
        }

        private void Start() => promptRoot.SetActive(false);
    }
}