using SubjectZero.Character.Player;

namespace SubjectZero.Interaction
{
    public interface IInteractable
    {
        /// <summary>Text shown in the prompt, e.g. "Open Door", "Read Note".</summary>
        string InteractionPrompt { get; }

        /// <summary>Allows conditional interaction (e.g. a locked door). Return true for always-interactable objects.</summary>
        bool CanInteract(PlayerController player);

        void Interact(PlayerController player);
    }
}