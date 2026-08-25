using System.Collections;
using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.Character.Enemy;
using SubjectZero.CameraSystem;
using SubjectZero.Audio;
using SubjectZero.Telemetry;
using SubjectZero.UI;
using SubjectZero.Interaction.Examples;

namespace SubjectZero.Core
{
    public class CaughtSequenceController : MonoBehaviour
    {
        public static CaughtSequenceController Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCameraController playerCamera;
        [SerializeField] private GameObject caughtMenuRoot;
        [SerializeField] private AudioClip jumpscareSfx;
        [SerializeField] private float faceDistance = 2f;
        [SerializeField] private float eyeHeightOffset = 1.5f;
        [SerializeField] private float sequenceDelay = 1.5f;

        public bool IsActive { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start() => caughtMenuRoot.SetActive(false);

        public void TriggerCaughtSequence(EnemyController entity)
        {
            IsActive = true;
            StartCoroutine(CaughtRoutine(entity));
        }

        private IEnumerator CaughtRoutine(EnemyController entity)
        {
            TextDocumentController.Instance?.ForceClose();
            AudioLogPickup.CurrentlyPlaying?.HandlePlayerCaughtFade();

            entity.FreezeForCaughtSequence();
            player.SetInputLocked(true);
            playerCamera.SetLocked(true);

            PositionPlayerFacingEntity(entity.transform);

            if (jumpscareSfx != null)
                AudioManager.Instance.PlaySfx3D(jumpscareSfx, entity.transform.position, 1f);

            float t = 0f;
            while (t < sequenceDelay)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }

            ShowCaughtMenu();
        }

        private void PositionPlayerFacingEntity(Transform entityTransform)
        {
            Vector3 toPlayerFlat = player.transform.position - entityTransform.position;
            toPlayerFlat.y = 0f;
            if (toPlayerFlat.sqrMagnitude < 0.01f) toPlayerFlat = -entityTransform.forward;

            Vector3 direction = toPlayerFlat.normalized;
            Vector3 targetPosition = entityTransform.position + direction * faceDistance;

            var cc = player.CharacterController;
            cc.enabled = false;
            player.transform.position = targetPosition;

            Vector3 lookTarget = entityTransform.position + Vector3.up * eyeHeightOffset;
            Vector3 flatLookDir = lookTarget - (targetPosition + Vector3.up * eyeHeightOffset);
            flatLookDir.y = 0f;
            player.transform.rotation = Quaternion.LookRotation(flatLookDir.normalized, Vector3.up);
            cc.enabled = true;

            playerCamera.SetForcedPitch(0f);
        }

        private void ShowCaughtMenu()
        {
            caughtMenuRoot.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnRetryButton()
        {
            IsActive = false;

            caughtMenuRoot.SetActive(false);
            GameManager.Instance.RespawnPlayerAtCheckpoint();
            GameManager.Instance.ReactivateEntityForCurrentZone();

            player.SetInputLocked(false);
            playerCamera.SetLocked(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void OnQuitButton()
        {
            Time.timeScale = 1f;
            TelemetryUploader.Instance?.UploadCurrentSession();
            LoadingScreenController.Instance.LoadSceneSingle("MainMenu");
        }
    }
}