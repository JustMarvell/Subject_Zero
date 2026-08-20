using UnityEngine;
using SubjectZero.Character.Player;

namespace SubjectZero.Interaction.Examples
{
    /// <summary>
    /// Stub for the story's environmental documents. Just logs for now - a proper
    /// note-reading UI panel is a later phase, but the interaction hook is real.
    /// </summary>
    public class NotePickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string noteTitle = "Untitled Note";
        [TextArea(3, 8)]
        [SerializeField] private string noteBody = "...";

        public string InteractionPrompt => $"Read '{noteTitle}'";

        public bool CanInteract(PlayerController player) => true;

        public void Interact(PlayerController player)
        {
            Debug.Log($"[Note] {noteTitle}\n{noteBody}");
            // TODO: hook into a real note-reading UI panel in a later phase
        }
    }
}