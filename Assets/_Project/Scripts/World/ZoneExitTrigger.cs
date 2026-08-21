using UnityEngine;
using SubjectZero.Core;

namespace SubjectZero.World
{
    [RequireComponent(typeof(Collider))]
    public class ZoneExitTrigger : MonoBehaviour
    {
        [SerializeField] private string nextZoneScene;
        [SerializeField] private string nextSpawnPointId;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            GameManager.Instance.TransitionToZone(nextZoneScene, nextSpawnPointId);
        }
    }
}