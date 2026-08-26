using UnityEngine;
using UnityEngine.Audio;
using SubjectZero.Core;
using SubjectZero.Audio;

namespace SubjectZero.Character.Enemy
{
    public class EnemyAudio : MonoBehaviour
    {
        [SerializeField] private EnemyController enemy;
        [SerializeField] private AudioClip patrolAmbience;
        [SerializeField] private AudioClip alertStinger;
        [SerializeField] private AudioClip alertLoop;
        [SerializeField] private AudioClip chaseStinger;
        [SerializeField] private AudioClip chaseLoop;
        [SerializeField] private AudioMixerGroup ambienceGroup;

        private const string LoopChannel = "entity_tension";

        private void OnEnable() => enemy.StateMachine.OnStateChanged += HandleStateChanged;
        private void OnDisable() => enemy.StateMachine.OnStateChanged -= HandleStateChanged;

        private void OnDestroy()
        {
            AudioManager.Instance?.StopLoop(LoopChannel, 0f);
        }

        private void HandleStateChanged(IState newState)
        {
            if (newState == enemy.PatrolState)
            {
                AudioManager.Instance.PlayLoop(LoopChannel, patrolAmbience, ambienceGroup, 0.5f, 1.5f);
            }
            else if (newState == enemy.AlertState)
            {
                AudioManager.Instance.PlaySfx3D(alertStinger, transform.position, 1f);
                AudioManager.Instance.PlayLoop(LoopChannel, alertLoop, ambienceGroup, 0.7f, 0.5f);
            }
            else if (newState == enemy.ChaseState)
            {
                AudioManager.Instance.PlaySfx3D(chaseStinger, transform.position, 1f);
                AudioManager.Instance.PlayLoop(LoopChannel, chaseLoop, ambienceGroup, 1f, 0.3f);
            }
            else if (newState == enemy.LostState)
            {
                AudioManager.Instance.StopLoop(LoopChannel, 1f);
            }
        }
    }
}