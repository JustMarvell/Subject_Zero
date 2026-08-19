using UnityEngine;

namespace SubjectZero.Character.Player
{
    /// <summary>
    /// Handles the crouch/standing stance: toggling, obstruction checks, and smoothly
    /// lerping the CharacterController's height/center and the camera pivot's height.
    /// Kept outside the state machine because crouching is an orthogonal concern that
    /// can apply during both Idle and Locomotion states.
    /// </summary>
    public class PlayerStance
    {
        private readonly PlayerController _player;

        public bool IsCrouching { get; private set; }

        public PlayerStance(PlayerController player)
        {
            _player = player;
        }

        public void ToggleCrouch()
        {
            if (IsCrouching)
            {
                if (CanStand())
                    IsCrouching = false;
                // else: stays crouched, will re-check next time ToggleCrouch is called
            }
            else
            {
                IsCrouching = true;
            }
        }

        public void Tick(float deltaTime)
        {
            var config = _player.Config;
            var cc = _player.CharacterController;
            var camPivot = _player.CameraPivot;

            float targetHeight = IsCrouching ? config.crouchingHeight : config.standingHeight;
            float targetCenterY = IsCrouching ? config.crouchingCenterY : config.standingCenterY;
            float targetCamHeight = IsCrouching ? config.cameraCrouchingHeight : config.cameraStandingHeight;

            float t = config.crouchTransitionSpeed * deltaTime;

            cc.height = Mathf.Lerp(cc.height, targetHeight, t);
            cc.center = new Vector3(cc.center.x, Mathf.Lerp(cc.center.y, targetCenterY, t), cc.center.z);

            Vector3 camLocal = camPivot.localPosition;
            camLocal.y = Mathf.Lerp(camLocal.y, targetCamHeight, t);
            camPivot.localPosition = camLocal;
        }

        private bool CanStand()
        {
            var config = _player.Config;
            var cc = _player.CharacterController;

            float clearanceNeeded = config.standingHeight - cc.height;
            if (clearanceNeeded <= 0f) return true;

            Vector3 capsuleTop = _player.transform.position + Vector3.up * cc.height;
            float radius = Mathf.Max(0.05f, cc.radius * 0.95f);

            bool blocked = Physics.SphereCast(
                capsuleTop, radius, Vector3.up, out _, clearanceNeeded,
                config.obstructionMask, QueryTriggerInteraction.Ignore);

            return !blocked;
        }
    }
}