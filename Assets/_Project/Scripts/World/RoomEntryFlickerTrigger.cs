using UnityEngine;

namespace SubjectZero.World
{
    [RequireComponent(typeof(Collider))]
    public class RoomEntryFlickerTrigger : MonoBehaviour
    {
        [SerializeField] private RandomFlickerController controller;
        [SerializeField] private FlickeringLightGroup targetGroup;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            controller.TryRoomEntryEvent(targetGroup);
        }
    }
}