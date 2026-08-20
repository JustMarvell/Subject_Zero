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

        public float CurrentPatrolSpeed { get; private set; }
        public float CurrentAlertSpeed { get; private set; }
        public float CurrentSearchSpeed { get; private set; }
        public float CurrentChaseSpeed { get; private set; }
        public float CurrentVisionRange { get; private set; }

        public bool IsInSafeState =>
            StateMachine.CurrentState == PatrolState || StateMachine.CurrentState == LostState;

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

            CurrentPatrolSpeed = config.patrolSpeed;
            CurrentAlertSpeed = config.alertSpeed;
            CurrentSearchSpeed = config.searchSpeed;
            CurrentChaseSpeed = config.chaseSpeed;
            CurrentVisionRange = config.visionRange;
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

        /// <summary>
        /// Applies a difficulty adjustment (-1 = too easy, +1 = too hard) to this entity's
        /// runtime stats, derived from the config's base values - never mutates the
        /// EnemyConfig asset itself. Only chase speed and vision range have explicit clamp
        /// ranges from the original design; the other movement speeds share the same
        /// speed factor for consistency but rely on the factor's own bounds rather than a
        /// separately-specified range (a TODO, same as the stress-score thresholds).
        /// </summary>
        public void ApplyDifficultyAdjustment(float score)
        {
            score = Mathf.Clamp(score, -1f, 1f);

            float speedFactor = 1f - 0.15f * score;
            CurrentPatrolSpeed = config.patrolSpeed * speedFactor;
            CurrentAlertSpeed = config.alertSpeed * speedFactor;
            CurrentSearchSpeed = config.searchSpeed * speedFactor;
            CurrentChaseSpeed = Mathf.Clamp(config.chaseSpeed * speedFactor, 2.8f, 4.2f);

            float visionFactor = 1f - 0.2f * score;
            CurrentVisionRange = Mathf.Clamp(config.visionRange * visionFactor, 5f, 10f);
        }
    }
}