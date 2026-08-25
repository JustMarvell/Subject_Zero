using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SubjectZero.Character.Player;

namespace SubjectZero.Telemetry
{
    public class TelemetryManager : MonoBehaviour
    {
        public static TelemetryManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Sampling")]
        [SerializeField] private float samplingInterval = 10f;
        [SerializeField] private float rollingWindowSeconds = 30f;

        [Header("Erratic Movement Detection")]
        [SerializeField] private float directionChangeAngleThreshold = 35f;

        public string SessionId { get; private set; }
        public float SessionTime { get; private set; }
        public string CurrentZone { get; set; } = "unassigned";

        public bool DDAEnabled { get; set; } = true;
        public bool TelemetryLoggingEnabled { get; set; } = true;
        public bool FlashlightRelevant { get; set; } = true;

        public int TotalDeaths => _deathTimestamps.Count;
        public int TotalNearMisses => _nearMissTimestamps.Count;
        public int TotalHidingTime => _hideIntervals.Count;
        public int TotalResourceConsumed => _resourceConsumedTimestamps.Count;

        private DDAWeights _weights;
        private string _outputPath;
        private float _samplingTimer;

        private readonly List<float> _deathTimestamps = new();
        private readonly List<float> _nearMissTimestamps = new();
        private readonly List<(float time, float ms)> _reactionSamples = new();
        private readonly List<(float start, float end)> _hideIntervals = new();
        private readonly List<(float start, float end)> _idleIntervals = new();
        private readonly List<float> _resourceConsumedTimestamps = new();
        private readonly List<float> _directionChangeTimestamps = new();
        private readonly List<(float start, float end)> _darkIntervals = new();
        private readonly List<(float start, float end)> _readingIntervals = new();

        private bool _isHiding;
        private bool _isReading;
        private float _currentHideStart;
        private float _currentReadingStart;
        private bool _isIdle;
        private bool _isDark;
        private float _currentIdleStart;
        private float _currentDarkStart;
        private Vector3 _lastMoveDir;

        private float? _pendingStimulusTime;
        private PlayerController.LocomotionMode _lastLocomotionMode;
        private bool _lastCrouchState;

        public event Action<TelemetrySample> OnSampleLogged;
        public TelemetrySample LatestSample { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            TelemetryUploader.Instance?.SetTelemetryManager(this);
            _weights = DDAWeights.Load();
        }

        private void Start() => StartSession();

        public void StartSession()
        {
            SessionId = Guid.NewGuid().ToString("N").Substring(0, 8);
            SessionTime = 0f;
            _samplingTimer = 0f;
            _pendingStimulusTime = null;
            _lastLocomotionMode = player.CurrentLocomotionMode;
            _lastCrouchState = player.Stance.IsCrouching;

            string folder = Path.Combine(Application.persistentDataPath, "TelemetrySessions");
            Directory.CreateDirectory(folder);
            _outputPath = Path.Combine(folder, $"session_{SessionId}.jsonl");

            Debug.Log($"[TelemetryManager] Session started: {SessionId}. Writing to {_outputPath}");
        }

        private void Update()
        {
            SessionTime += Time.deltaTime;
            TrackMovementErraticism();
            TrackIdleState();
            TrackReactionWindow();

            _samplingTimer += Time.deltaTime;
            if (_samplingTimer >= samplingInterval)
            {
                _samplingTimer = 0f;
                if (TelemetryLoggingEnabled)
                    SampleAndLog();
            }
        }

        private void TrackMovementErraticism()
        {
            Vector3 dir = player.MoveVelocity;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;

            dir.Normalize();
            if (_lastMoveDir.sqrMagnitude > 0.01f)
            {
                float angle = Vector3.Angle(_lastMoveDir, dir);
                if (angle >= directionChangeAngleThreshold)
                    _directionChangeTimestamps.Add(SessionTime);
            }
            _lastMoveDir = dir;
        }

        private void TrackIdleState()
        {
            bool idleNow = player.CurrentLocomotionMode == PlayerController.LocomotionMode.Idle;
            if (idleNow && !_isIdle)
            {
                _isIdle = true;
                _currentIdleStart = SessionTime;
            }
            else if (!idleNow && _isIdle)
            {
                _isIdle = false;
                _idleIntervals.Add((_currentIdleStart, SessionTime));
            }
        }

        /// <summary>
        /// Watches for the player's first meaningful reaction (a movement-mode
        /// change or crouch toggle) after a stimulus was armed, and logs the
        /// elapsed time as a reaction sample.
        /// </summary>
        private void TrackReactionWindow()
        {
            bool modeChanged = player.CurrentLocomotionMode != _lastLocomotionMode;
            bool crouchChanged = player.Stance.IsCrouching != _lastCrouchState;

            if (_pendingStimulusTime.HasValue && (modeChanged || crouchChanged))
            {
                float reactionMs = (SessionTime - _pendingStimulusTime.Value) * 1000f;
                _reactionSamples.Add((SessionTime, reactionMs));
                _pendingStimulusTime = null;
            }

            _lastLocomotionMode = player.CurrentLocomotionMode;
            _lastCrouchState = player.Stance.IsCrouching;
        }

        // ----- Public recording API for other systems -----

        public void RecordDeath() => _deathTimestamps.Add(SessionTime);

        public void RecordNearMiss() => _nearMissTimestamps.Add(SessionTime);

        /// <summary>Call the moment a threat/stimulus first appears (e.g. entity enters Alert).</summary>
        public void ArmReactionWindow()
        {
            if (player != null && player.InputLocked) return; // can't react if input is frozen
            if (!_pendingStimulusTime.HasValue)
                _pendingStimulusTime = SessionTime;
        }

        public void RecordHideStart()
        {
            if (_isHiding) return;
            _isHiding = true;
            _currentHideStart = SessionTime;
        }

        public void RecordHideEnd()
        {
            if (!_isHiding) return;
            _isHiding = false;
            _hideIntervals.Add((_currentHideStart, SessionTime));
        }

        public void RecordDarknessStart()
        {
            if (_isDark) return;
            _isDark = true;
            _currentDarkStart = SessionTime;
        }

        public void RecordDarknessEnd()
        {
            if (!_isDark) return;
            _isDark = false;
            _darkIntervals.Add((_currentDarkStart, SessionTime));
        }

        public void RecordReadingStart()
        {
            if (_isReading) return;
            _isReading = true;
            _currentReadingStart = SessionTime;
        }

        public void RecordReadingEnd()
        {
            if (!_isReading) return;
            _isReading = false;
            _readingIntervals.Add((_currentReadingStart, SessionTime));
        }

        public void ClearPendingReaction() => _pendingStimulusTime = null;

        public void RecordResourceConsumed() => _resourceConsumedTimestamps.Add(SessionTime);

        // ----- Windowed feature computation -----

        private void SampleAndLog()
        {
            float windowStart = SessionTime - rollingWindowSeconds;

            var sample = new TelemetrySample
            {
                session_id = SessionId,
                session_time = SessionTime,
                zone = CurrentZone,

                death_rate = RatePerMinute(_deathTimestamps, windowStart),
                near_miss_rate = RatePerMinute(_nearMissTimestamps, windowStart),
                avg_reaction_time = AverageReactionTime(windowStart),
                hide_ratio = IntervalRatio(_hideIntervals, _isHiding, _currentHideStart, windowStart),
                movement_erraticism = RatePerMinute(_directionChangeTimestamps, windowStart),
                resource_usage_rate = RatePerMinute(_resourceConsumedTimestamps, windowStart),
                idle_ratio = IntervalRatio(_idleIntervals, _isIdle, _currentIdleStart, windowStart),
                reading_ratio = IntervalRatio(_readingIntervals, _isReading, _currentReadingStart, windowStart),
                darkness_ratio = IntervalRatio(_darkIntervals, _isDark, _currentDarkStart, windowStart)
            };

            sample.stress_score = StressScoreCalculator.ComputeScore(sample, _weights);
            sample.difficulty_label = StressScoreCalculator.ComputeLabel(sample.stress_score, _weights);

            AppendToFile(sample);

            LatestSample = sample;
            OnSampleLogged?.Invoke(sample);
        }

        private float RatePerMinute(List<float> timestamps, float windowStart)
        {
            int count = 0;
            foreach (float t in timestamps)
                if (t >= windowStart) count++;
            return count / (rollingWindowSeconds / 60f);
        }

        private float AverageReactionTime(float windowStart)
        {
            float sum = 0f;
            int count = 0;
            foreach (var (time, ms) in _reactionSamples)
            {
                if (time >= windowStart) { sum += ms; count++; }
            }
            return count > 0 ? sum / count : 0f;
        }

        private float IntervalRatio(List<(float start, float end)> closedIntervals, bool isOpen, float openStart, float windowStart)
        {
            float total = 0f;
            foreach (var (start, end) in closedIntervals)
            {
                float overlapStart = Mathf.Max(start, windowStart);
                float overlapEnd = Mathf.Min(end, SessionTime);
                if (overlapEnd > overlapStart) total += overlapEnd - overlapStart;
            }
            if (isOpen)
            {
                float overlapStart = Mathf.Max(openStart, windowStart);
                total += SessionTime - overlapStart;
            }
            return Mathf.Clamp01(total / rollingWindowSeconds);
        }

        private void AppendToFile(TelemetrySample sample)
        {
            string json = JsonUtility.ToJson(sample);
            try
            {
                File.AppendAllText(_outputPath, json + "\n");
            }
            catch (Exception e)
            {
                Debug.LogError($"[TelemetryManager] Failed to write sample: {e.Message}");
            }
        }
    }
}