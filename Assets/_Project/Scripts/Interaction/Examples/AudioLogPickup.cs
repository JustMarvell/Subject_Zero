using UnityEngine;
using SubjectZero.Character.Player;

namespace SubjectZero.Interaction.Examples
{
    /// <summary>
    /// Plays the sibling's audio logs per the story design. Real "log inbox" UI is a
    /// later phase - for now this just plays the clip in-world via an AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioLogPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string logTitle = "Untitled Log";
        private AudioSource _audioSource;
        private bool _hasPlayed;

        public string InteractionPrompt => _hasPlayed ? $"Replay '{logTitle}'" : $"Play '{logTitle}'";
        public bool CanInteract(PlayerController player) => true;

        private void Awake() => _audioSource = GetComponent<AudioSource>();

        public void Interact(PlayerController player)
        {
            _audioSource.Play();
            _hasPlayed = true;
        }
    }
}