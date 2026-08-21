using MimicSpace;
using UnityEngine;
using UnityEngine.AI;

namespace SubjectZero.Character.Enemy
{
    public class MimicVelocityUpdater : MonoBehaviour
    {
        [SerializeField] private Mimic mimic;
        [SerializeField] private NavMeshAgent agent;


        void Update()
        {
            mimic.velocity = agent.velocity;    
        }
    }
}