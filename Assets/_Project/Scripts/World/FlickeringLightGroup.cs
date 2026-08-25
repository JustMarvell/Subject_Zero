using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SubjectZero.Audio;

namespace SubjectZero.World
{
    public class FlickeringLightGroup : MonoBehaviour
    {
        [SerializeField] private List<Light> lights = new();
        [SerializeField] private Light directionalLight; // optional - leave unassigned for per-room groups
        [SerializeField] private float directionalDimIntensity = 0.05f;
        [SerializeField] private float directionalNormalIntensity = 0.2f;
        [SerializeField] private float flickerDuration = 0.6f;
        [SerializeField] private int flickerSteps = 6;
        [SerializeField] private AudioClip flickerSfx;

        public bool IsOn { get; private set; } = true;

        public IEnumerator PlayFlicker(bool endState)
        {
            if (flickerSfx != null)
                AudioManager.Instance.PlaySfx3D(flickerSfx, transform.position, 1f);

            float stepDuration = flickerDuration / Mathf.Max(1, flickerSteps);
            for (int i = 0; i < flickerSteps; i++)
            {
                SetLights(i % 2 == 0);
                yield return new WaitForSeconds(stepDuration);
            }

            SetLights(endState);
            IsOn = endState;

            if (directionalLight != null)
                directionalLight.intensity = endState ? directionalNormalIntensity : directionalDimIntensity;
        }

        private void SetLights(bool on)
        {
            foreach (var light in lights)
                if (light != null) light.enabled = on;
        }
    }
}