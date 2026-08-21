using UnityEngine;
using SubjectZero.Character.Player;

namespace SubjectZero.Interaction.Examples
{
    public class SimpleDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float rotateSpeed = 4f;
        [SerializeField] private Transform hinge;

        private Quaternion _closedRotation;
        private Quaternion _openRotation;
        private bool _isOpen;

        public string InteractionPrompt => _isOpen ? "Close Door" : "Open Door";

        private void Awake()
        {
            _closedRotation = hinge != null ? hinge.transform.localRotation : transform.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        }

        public bool CanInteract(PlayerController player) => true;

        public void Interact(PlayerController player) => _isOpen = !_isOpen;

        private void Update()
        {
            Quaternion target = _isOpen ? _openRotation : _closedRotation;

            if (hinge == null)
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, rotateSpeed * Time.deltaTime);
            else
                hinge.localRotation = Quaternion.Slerp(hinge.localRotation, target, rotateSpeed * Time.deltaTime);
        }
    }
}