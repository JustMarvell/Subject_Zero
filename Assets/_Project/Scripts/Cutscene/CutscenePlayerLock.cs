using UnityEngine;
using SubjectZero.Character.Player;
using SubjectZero.CameraSystem;

namespace SubjectZero.Cutscene
{
    /// <summary>Thin parameterless wrappers so a Signal Receiver can call these directly.</summary>
    public class CutscenePlayerLock : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCameraController playerCamera;

        public void LockPlayer()
        {
            player.SetInputLocked(true);
            playerCamera.SetLocked(true);
        }

        public void UnlockPlayer()
        {
            player.SetInputLocked(false);
            playerCamera.SetLocked(false);
        }
    }
}