
using UnityEngine;
using SubjectZero.Input;
using SubjectZero.Character.Player;

namespace SubjectZero.CameraSystem
{
    /// <summary>
    /// Drives first-person look directly (not through Cinemachine Aim/Body).
    /// The CinemachineCamera should be parented under the same cameraPivot this
    /// script rotates, so it inherits position/rotation through normal Unity
    /// parenting. Cinemachine's own job is left free for lens/FOV, impulse
    /// (jump-scare shake), and noise (idle sway) - added in a later phase.
    /// </summary>
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

            float yaw = look.x * config.lookSensitivityX * Time.deltaTime;
            _pitch -= look.y * config.lookSensitivityY * Time.deltaTime;
            _pitch = Mathf.Clamp(_pitch, config.pitchClampMin, config.pitchClampMax);

            playerBody.Rotate(Vector3.up, yaw);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }
    }
}