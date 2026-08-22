using UnityEngine;
using UnityEngine.AI;
using SubjectZero.Character.Player;

namespace SubjectZero.Interaction.Examples
{
    public class LockedDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private string requiredItemId = "zone1_keycard";
        [SerializeField] private float openAngle = 90f;
        [SerializeField] private float rotateSpeed = 4f;
        [SerializeField] private NavMeshObstacle navObstacle;
        [SerializeField] private Transform hinge;

        private Quaternion _closedRotation, _openRotation;
        private bool _isOpen;

        public string InteractionPrompt =>
            PlayerKeyItems.Instance != null && PlayerKeyItems.Instance.HasItem(requiredItemId)
                ? (_isOpen ? "Close Door" : "Open Door")
                : "Locked";

        private void Awake()
        {
            _closedRotation = hinge != null ? hinge.transform.localRotation : transform.localRotation;
            _openRotation = _closedRotation * Quaternion.Euler(0f, openAngle, 0f);
            SyncObstacle();
        }

        public bool CanInteract(PlayerController player) =>
            PlayerKeyItems.Instance != null && PlayerKeyItems.Instance.HasItem(requiredItemId);

        public void Interact(PlayerController player) => SetOpen(!_isOpen);

        /// <summary>Bypasses the key check entirely - used only by the entity.</summary>
        public void EntityOpen() => SetOpen(true);
        public void EntityClose() => SetOpen(false);

        private void SetOpen(bool open)
        {
            _isOpen = open;
            SyncObstacle();
        }

        private void SyncObstacle()
        {
            if (navObstacle != null)
                navObstacle.enabled = !_isOpen;
        }

        private void Update()
        {
            Quaternion target = _isOpen ? _openRotation : _closedRotation;
            if (hinge == null)
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, rotateSpeed * Time.deltaTime);
            else
                hinge.transform.localRotation = Quaternion.Slerp(hinge.transform.localRotation, target, rotateSpeed * Time.deltaTime);
        }   
    }
}