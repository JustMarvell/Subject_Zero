using TMPro;
using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.CameraSystem;
using SubjectZero.Telemetry;
using SubjectZero.Input;
using SubjectZero.Story;

namespace SubjectZero.UI
{
    public class TextDocumentController : MonoBehaviour
    {
        public static TextDocumentController Instance { get; private set; }

        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCameraController playerCamera;
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private GameObject documentRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private TMP_Text pageIndicatorText;

        public bool IsReading { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            documentRoot.SetActive(false);
            bodyText.overflowMode = TextOverflowModes.Page; // enforced here so it can't be left misconfigured in the Inspector
        }

        private void Update()
        {
            // Reuses the Pause input action's binding (Escape) - closes the
            // document instead of opening Pause while reading is active.
            if (IsReading && inputReader.PausePressedThisFrame)
                Close();
        }

        public void Open(TextDocumentData data)
        {
            IsReading = true;
            titleText.text = data.documentTitle;
            bodyText.text = data.bodyText;
            bodyText.pageToDisplay = 1;
            UpdatePageIndicator();

            player.SetInputLocked(true);
            playerCamera.SetLocked(true);
            documentRoot.SetActive(true);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            TelemetryManager.Instance?.ClearPendingReaction();
            TelemetryManager.Instance?.RecordReadingStart();
        }

        public void OnNextPage()
        {
            if (bodyText.pageToDisplay < bodyText.textInfo.pageCount)
                bodyText.pageToDisplay++;
            UpdatePageIndicator();
        }

        public void OnPreviousPage()
        {
            if (bodyText.pageToDisplay > 1)
                bodyText.pageToDisplay--;
            UpdatePageIndicator();
        }

        private void UpdatePageIndicator()
        {
            if (pageIndicatorText != null)
                pageIndicatorText.text = $"{bodyText.pageToDisplay}";
        }

        public void Close()
        {
            if (!IsReading) return;
            IsReading = false;

            documentRoot.SetActive(false);
            player.SetInputLocked(false);
            playerCamera.SetLocked(false);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            TelemetryManager.Instance?.RecordReadingEnd();
        }

        /// <summary>Called by CaughtSequenceController/GameCompleteController -
        /// deliberately doesn't touch cursor/lock state, since whichever of
        /// those triggered this takes over both immediately after.</summary>
        public void ForceClose()
        {
            if (!IsReading) return;
            IsReading = false;
            documentRoot.SetActive(false);
            TelemetryManager.Instance?.RecordReadingEnd();
        }
    }
}