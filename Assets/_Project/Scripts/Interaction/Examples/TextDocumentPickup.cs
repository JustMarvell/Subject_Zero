using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.UI;
using SubjectZero.Story;

namespace SubjectZero.Interaction.Examples
{
    public class TextDocumentPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private TextDocumentData data;

        public string InteractionPrompt => $"Read '{data.documentTitle}'";
        public bool CanInteract(PlayerController player) => data != null;
        public void Interact(PlayerController player) => TextDocumentController.Instance.Open(data);
    }
}