using UnityEngine;
using SubjectZero.Core;

namespace SubjectZero.World
{
    [RequireComponent(typeof(Collider))]
    public class GameCompleteTrigger : MonoBehaviour
    {
        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            GameCompleteController.Instance.TriggerGameComplete();
        }
    }
}