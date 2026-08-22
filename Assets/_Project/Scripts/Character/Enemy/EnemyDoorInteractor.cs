using System.Collections.Generic;
using UnityEngine;
using SubjectZero.Interaction.Examples;

namespace SubjectZero.Character.Enemy
{
    public class EnemyDoorInteractor : MonoBehaviour
    {
        [SerializeField] private float doorInteractRange = 2f;
        [SerializeField] private float doorReleaseRange = 3.5f;
        [SerializeField] private LayerMask doorMask = ~0;

        private readonly Dictionary<LockedDoor, bool> _heldOpenDoors = new();

        private void Update()
        {
            CheckNearbyDoors();
            ReleaseFarDoors();
        }

        private void CheckNearbyDoors()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, doorInteractRange, doorMask, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                var door = hit.GetComponentInParent<LockedDoor>();
                if (door == null || _heldOpenDoors.ContainsKey(door)) continue;

                door.EntityOpen();
                _heldOpenDoors[door] = true;
            }
        }

        private void ReleaseFarDoors()
        {
            if (_heldOpenDoors.Count == 0) return;

            List<LockedDoor> toRelease = null;
            foreach (var door in _heldOpenDoors.Keys)
            {
                if (door == null) continue;
                if (Vector3.Distance(transform.position, door.transform.position) > doorReleaseRange)
                    (toRelease ??= new List<LockedDoor>()).Add(door);
            }

            if (toRelease == null) return;
            foreach (var door in toRelease)
            {
                door.EntityClose();
                _heldOpenDoors.Remove(door);
            }
        }
    }
}