using UnityEngine;
using TMPro;
using SubjectZero.Character.Player;
using SubjectZero.Character.Enemy;
using SubjectZero.CameraSystem;
using SubjectZero.Telemetry;
using SubjectZero.UI;
using SubjectZero.Interaction.Examples;
using SubjectZero.Audio;

namespace SubjectZero.Core
{
    public class GameCompleteController : MonoBehaviour
    {
        public static GameCompleteController Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCameraController playerCamera;
        [SerializeField] private EnemyController entity;
        [SerializeField] private TelemetryManager telemetryManager;
        [SerializeField] private GameObject completeMenuRoot;
        [SerializeField] private TMP_Text statsText;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => completeMenuRoot.SetActive(false);

        public void TriggerGameComplete()
        {
            AudioManager.Instance.StopLoop("entity_tension", 1f);
            AudioManager.Instance.StopLoop("music", 1f);

            TextDocumentController.Instance?.ForceClose();
            AudioLogPickup.CurrentlyPlaying?.HandlePlayerCaughtFade();

            IsActive = true;
            player.SetInputLocked(true);
            playerCamera.SetLocked(true);

            Time.timeScale = 0f;

            PopulateStats();

            TelemetryUploader.Instance?.UploadCurrentSession();

            completeMenuRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void PopulateStats()
        {
            if (statsText == null) return;

            int minutes = Mathf.FloorToInt(telemetryManager.SessionTime / 60f);
            int seconds = Mathf.FloorToInt(telemetryManager.SessionTime % 60f);

            statsText.text =
                $"Time Survived: {minutes:00}:{seconds:00}\n" +
                $"Times Caught: {telemetryManager.TotalDeaths}\n" +
                $"Near Misses: {telemetryManager.TotalNearMisses}\n" +
                $"Total Hide times: {telemetryManager.TotalHidingTime}\n" +
                $"Total Resource Used: {telemetryManager.TotalResourceConsumed}\n" +
                $"Final Entity Chase Speed: {entity.CurrentChaseSpeed:F1} m/s\n" +
                $"Final Entity Vision Range: {entity.CurrentVisionRange:F1} m";
        }

        // Wire to the Play Again button's OnClick
        public void OnPlayAgainButton()
        {
            IsActive = false;
            Time.timeScale = 1f;
            LoadingScreenController.Instance.LoadSceneSingle("Bootstrap", () =>
                GameManager.Instance.LoadFirstZone(null));
        }

        // Wire to the Quit to Main Menu button's OnClick
        public void OnQuitButton()
        {
            IsActive = false;
            Time.timeScale = 1f;
            LoadingScreenController.Instance.LoadSceneSingle("MainMenu");
        }
    }
}