using System;
using UnityEngine;
using UnityEngine.AI;
using SubjectZero.Core;
using SubjectZero.Character.Player;
using SubjectZero.Telemetry;

namespace SubjectZero.Character.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyConfig config;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PatrolRoute patrolRoute;

        public EnemyConfig Config => config;
        public Transform PlayerTransform => playerTransform;
        public PlayerController Player => playerController;
        public PatrolRoute PatrolRoute => patrolRoute;
        public NavMeshAgent Agent { get; private set; }
        public StateMachine StateMachine { get; private set; }
        public EnemyPerception Perception { get; private set; }
        public Vector3 LastKnownPlayerPosition { get; set; }

        public EnemyPatrolState PatrolState { get; private set; }
        public EnemyAlertState AlertState { get; private set; }
        public EnemySearchState SearchState { get; private set; }
        public EnemyChaseState ChaseState { get; private set; }
        public EnemyLostState LostState { get; private set; }

        /// <summary>Stub hook - later phases (GameManager, respawn/checkpoint system) will subscribe here.</summary>
        public event Action OnPlayerCaught;

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            StateMachine = new StateMachine();
            Perception = new EnemyPerception(this);

            PatrolState = new EnemyPatrolState(this);
            AlertState = new EnemyAlertState(this);
            SearchState = new EnemySearchState(this);
            ChaseState = new EnemyChaseState(this);
            LostState = new EnemyLostState(this);
        }

        private void Start()
        {
            StateMachine.ChangeState(PatrolState);
        }

        private void Update()
        {
            StateMachine.Tick();
        }

        private void FixedUpdate()
        {
            StateMachine.FixedTick();
        }

        public void TriggerCatch()
        {
            TelemetryManager.Instance?.RecordDeath();
            OnPlayerCaught?.Invoke();
            Debug.Log("[EnemyController] Player caught. (Retry/respawn not yet built - stub only.)");
        }
    }
}