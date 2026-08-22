using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.Audio;

namespace SubjectZero.Interaction.Examples
{
    public class KeycardPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string itemId = "zone1_keycard";
        [SerializeField] private string displayName = "Keycard";
        [SerializeField] private AudioClip pickupClip;

        public string InteractionPrompt => $"Pick Up {displayName}";
        public bool CanInteract(PlayerController player) => true;

        public void Interact(PlayerController player)
        {
            PlayerKeyItems.Instance.AddItem(itemId);
            AudioManager.Instance.PlaySfx3D(pickupClip, transform.position);
            Destroy(gameObject);
        }
    }
}