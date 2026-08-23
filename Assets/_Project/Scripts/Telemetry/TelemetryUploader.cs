using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace SubjectZero.Telemetry
{
    public class TelemetryUploader : MonoBehaviour
    {
        public static TelemetryUploader Instance { get; private set; }

        [SerializeField] private string uploadUrl = "https://script.google.com/macros/s/YOUR_DEPLOYMENT_ID/exec";
        [SerializeField] private float periodicUploadInterval = 120f;

        private TelemetryManager _telemetryManager;
        private float _timer;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Called by each new TelemetryManager on Awake, since this uploader
        /// persists across scene reloads (e.g. Play Again) but the TelemetryManager
        /// it should upload from gets recreated each time.</summary>
        public void SetTelemetryManager(TelemetryManager manager) => _telemetryManager = manager;

        private void Update()
        {
            if (_telemetryManager == null) return;

            _timer += Time.unscaledDeltaTime;
            if (_timer >= periodicUploadInterval)
            {
                _timer = 0f;
                UploadCurrentSession();
            }
        }

        public void UploadCurrentSession()
        {
            if (_telemetryManager == null || string.IsNullOrEmpty(_telemetryManager.SessionId)) return;
            StartCoroutine(UploadRoutine(_telemetryManager.SessionId));
        }

        private IEnumerator UploadRoutine(string sessionId)
        {
            string path = Path.Combine(Application.persistentDataPath, "TelemetrySessions", $"session_{sessionId}.jsonl");
            if (!File.Exists(path)) yield break;

            string content;
            try { content = File.ReadAllText(path); }
            catch (Exception e)
            {
                Debug.LogWarning($"[TelemetryUploader] Could not read session file: {e.Message}");
                yield break;
            }

            string url = $"{uploadUrl}?session_id={UnityWebRequest.EscapeURL(sessionId)}";
            using var request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(content);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "text/plain");

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
                Debug.LogWarning($"[TelemetryUploader] Upload failed: {request.error}");
            else
                Debug.Log("[TelemetryUploader] Session uploaded successfully.");
        }

        private void OnApplicationQuit()
        {
            // Best-effort only - Unity doesn't guarantee async requests finish
            // before the process exits. The periodic timer is the real safety net.
            UploadCurrentSession();
        }
    }
}