using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.Character.Enemy;
using SubjectZero.Telemetry;
using SubjectZero.World;
using SubjectZero.Audio;
using UnityEngine.SceneManagement;

namespace SubjectZero.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private EnemyController entity;
        [SerializeField] private TelemetryManager telemetryManager;
        [SerializeField] private string firstZoneScene = "Zone1_Reception";
        [SerializeField] private string firstSpawnPointId = "ZoneStart";

        private Vector3 _checkpointPosition;
        private Quaternion _checkpointRotation;
        private string _currentZoneScene;
        private string _pendingZoneScene;
        private string _pendingSpawnPointId;

        public EnemyController Entity => entity;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void LoadFirstZone(System.Action onComplete)
        {
            _pendingZoneScene = firstZoneScene;
            _pendingSpawnPointId = firstSpawnPointId;
            LoadingScreenController.Instance.LoadSceneAdditive(firstZoneScene, null, () =>
            {
                OnZoneLoaded();
                onComplete?.Invoke();
            });
        }

        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            _checkpointPosition = position;
            _checkpointRotation = rotation;
        }

        public void RespawnPlayerAtCheckpoint()
        {
            var cc = player.CharacterController;
            cc.enabled = false;
            player.transform.SetPositionAndRotation(_checkpointPosition, _checkpointRotation);
            cc.enabled = true;
        }

        public void TransitionToZone(string sceneName, string spawnPointId)
        {
            _pendingZoneScene = sceneName;
            _pendingSpawnPointId = spawnPointId;
            LoadingScreenController.Instance.LoadSceneAdditive(sceneName, _currentZoneScene, OnZoneLoaded);
        }

        private void OnZoneLoaded()
        {
            _currentZoneScene = _pendingZoneScene;

            SpawnPoint spawn = FindSpawnPoint(_pendingSpawnPointId);
            if (spawn == null)
            {
                Debug.LogError($"[GameManager] Spawn point '{_pendingSpawnPointId}' not found.");
                return;
            }

            var cc = player.CharacterController;
            cc.enabled = false;
            player.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
            cc.enabled = true;

            SetCheckpoint(spawn.transform.position, spawn.transform.rotation);

            var patrolRoute = FindZonePatrolRoute();
            bool zoneHasEntity = patrolRoute != null;

            if (entity != null)
            {
                if (zoneHasEntity) entity.SetPatrolRoute(patrolRoute);
                entity.SetZoneActive(zoneHasEntity);
            }

            MusicController.Instance?.HandleZoneLoaded(_currentZoneScene, entity);

            if (telemetryManager != null)
            {
                telemetryManager.CurrentZone = _currentZoneScene;
                telemetryManager.DDAEnabled = zoneHasEntity;
            }
        }

        private SpawnPoint FindSpawnPoint(string id)
        {
            foreach (var sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
                if (sp.SpawnId == id) return sp;
            return null;
        }

        private PatrolRoute FindZonePatrolRoute()
        {
            var routes = FindObjectsByType<PatrolRoute>(FindObjectsSortMode.None);
            return routes.Length > 0 ? routes[0] : null;
        }

        public void ReactivateEntityForCurrentZone()
        {
            var patrolRoute = FindZonePatrolRoute();
            bool zoneHasEntity = patrolRoute != null;

            if (entity != null)
            {
                if (zoneHasEntity) entity.SetPatrolRoute(patrolRoute);
                entity.SetZoneActive(zoneHasEntity);
            }
        }
    }
}