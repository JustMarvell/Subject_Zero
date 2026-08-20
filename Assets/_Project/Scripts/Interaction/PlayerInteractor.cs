using System;
using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.Input;

namespace SubjectZero.Interaction
{
    /// <summary>
    /// Raycasts from the camera pivot each frame looking for an IInteractable.
    /// GetComponentInParent is used (not GetComponent) so the collider can live on
    /// a child mesh while the IInteractable script sits on the root object.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform rayOrigin;
        [SerializeField] private float interactRange = 2.5f;
        [SerializeField] private LayerMask interactableMask = ~0;

        public event Action<IInteractable> OnFocusChanged;
        public IInteractable CurrentInteractable { get; private set; }

        private void Update()
        {
            UpdateFocus();

            if (inputReader.InteractPressedThisFrame && CurrentInteractable != null &&
                CurrentInteractable.CanInteract(player))
            {
                CurrentInteractable.Interact(player);
            }
        }

        private void UpdateFocus()
        {
            IInteractable found = null;

            if (Physics.Raycast(rayOrigin.position, rayOrigin.forward, out RaycastHit hit,
                    interactRange, interactableMask, QueryTriggerInteraction.Ignore))
            {
                found = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (!ReferenceEquals(found, CurrentInteractable))
            {
                CurrentInteractable = found;
                OnFocusChanged?.Invoke(CurrentInteractable);
            }
        }
    }
}