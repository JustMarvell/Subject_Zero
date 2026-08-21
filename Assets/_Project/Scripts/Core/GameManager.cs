using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using SubjectZero.Character.Player;
using SubjectZero.Character.Enemy;
using SubjectZero.Telemetry;
using SubjectZero.World;

namespace SubjectZero.Core
{
    /// <summary>
    /// Persists across zone transitions (lives in the Bootstrap scene). Owns the
    /// current checkpoint, respawn logic, and additive zone loading/unloading.
    /// </summary>
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

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(LoadZone(firstZoneScene, firstSpawnPointId, isFirstLoad: true));
        }

        public void SetCheckpoint(Vector3 position, Quaternion rotation)
        {
            _checkpointPosition = position;
            _checkpointRotation = rotation;
        }

        public void RespawnPlayerAtCheckpoint()
        {
            var cc = player.CharacterController;
            cc.enabled = false; // CharacterController fights direct transform writes unless briefly disabled
            player.transform.SetPositionAndRotation(_checkpointPosition, _checkpointRotation);
            cc.enabled = true;
        }

        public void TransitionToZone(string sceneName, string spawnPointId)
        {
            StartCoroutine(LoadZone(sceneName, spawnPointId, isFirstLoad: false));
        }

        private IEnumerator LoadZone(string sceneName, string spawnPointId, bool isFirstLoad)
        {
            if (!isFirstLoad && !string.IsNullOrEmpty(_currentZoneScene))
                yield return SceneManager.UnloadSceneAsync(_currentZoneScene);

            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _currentZoneScene = sceneName;

            SpawnPoint spawn = FindSpawnPoint(spawnPointId);
            if (spawn == null)
            {
                Debug.LogError($"[GameManager] Spawn point '{spawnPointId}' not found in {sceneName}.");
                yield break;
            }

            var cc = player.CharacterController;
            cc.enabled = false;
            player.transform.SetPositionAndRotation(spawn.transform.position, spawn.transform.rotation);
            cc.enabled = true;

            SetCheckpoint(spawn.transform.position, spawn.transform.rotation);

            if (telemetryManager != null)
                telemetryManager.CurrentZone = sceneName;

            var patrolRoute = FindZonePatrolRoute();
            if (patrolRoute != null && entity != null)
                entity.SetPatrolRoute(patrolRoute);
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
    }
}