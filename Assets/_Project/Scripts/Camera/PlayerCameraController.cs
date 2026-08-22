using UnityEngine;
using SubjectZero.Input;
using SubjectZero.Character.Player;
using SubjectZero.Core;

namespace SubjectZero.CameraSystem
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private PlayerInputReader inputReader;
        [SerializeField] private Transform playerBody;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private PlayerMovementConfig config;

        private float _pitch;

        private void Update()
        {
            Vector2 look = inputReader.LookInput;
            float sensMult = SettingsManager.Instance != null ? SettingsManager.Instance.SensitivityMultiplier : 1f;
            float invertSign = SettingsManager.Instance != null && SettingsManager.Instance.InvertY ? 1f : -1f;

            float yaw = look.x * config.lookSensitivityX * sensMult * Time.deltaTime;
            _pitch += invertSign * look.y * config.lookSensitivityY * sensMult * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, config.pitchClampMin, config.pitchClampMax);

            playerBody.Rotate(Vector3.up, yaw);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}