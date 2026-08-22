using UnityEngine;
using SubjectZero.Audio;

namespace SubjectZero.Character.Player
{
    public class FootstepController : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private AudioClip[] footstepClips;
        [SerializeField] private float walkStepInterval = 0.5f;
        [SerializeField] private float sneakStepInterval = 0.8f;
        [SerializeField] private float sprintStepInterval = 0.32f;

        private float _stepTimer;

        private void Update()
        {
            if (player.CurrentLocomotionMode == PlayerController.LocomotionMode.Idle)
            {
                _stepTimer = 0f;
                return;
            }

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= GetIntervalForMode())
            {
                _stepTimer = 0f;
                PlayFootstep();
            }
        }

        private float GetIntervalForMode() => player.CurrentLocomotionMode switch
        {
            PlayerController.LocomotionMode.Sneak => sneakStepInterval,
            PlayerController.LocomotionMode.Sprint => sprintStepInterval,
            _ => walkStepInterval
        };

        private void PlayFootstep()
        {
            if (footstepClips == null || footstepClips.Length == 0) return;
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            AudioManager.Instance.PlaySfx3D(clip, transform.position, player.CurrentNoiseLevel01);
        }
    }
}