using UnityEngine;
using SubjectZero.Character.Player;

namespace SubjectZero.Interaction.Examples
{
    public class KeycardPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemId = "zone1_keycard";
        [SerializeField] private string displayName = "Keycard";

        public string InteractionPrompt => $"Pick Up {displayName}";
        public bool CanInteract(PlayerController player) => true;

        public void Interact(PlayerController player)
        {
            PlayerKeyItems.Instance.AddItem(itemId);
            Destroy(gameObject);
        }
    }
}