using UnityEngine;
using SubjectZero.Core;

namespace SubjectZero.World
{
    [RequireComponent(typeof(Collider))]
    public class ZoneCheckpoint : MonoBehaviour
    {
        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            GameManager.Instance.SetCheckpoint(transform.position, transform.rotation);
        }
    }
}