using UnityEngine;

namespace SubjectZero.World
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId;
        public string SpawnId => spawnId;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        }
    }
}