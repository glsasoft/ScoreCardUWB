using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace Mirza.Solaris
{
    public class DemoController : MonoBehaviour
    {
        public Toggle toggleFireSmoke;
        public Toggle toggleGroundFog;

        [Space]

        public Toggle toggleGroundFogLights;
        public Toggle toggleGroundFogParticleLights;

        [Space]

        public Slider groundFogAdditionalLightingSlider;

        [Space]

        public GameObject[] fireSmoke;

        [Space]

        public GameObject groundFog;
        public GameObject groundFogLights;

        [Space]

        public ParticleSystem groundFogParticleLights;

        [Space]

        public Renderer[] groundFogRenderers;

        void Start()
        {
            toggleFireSmoke.onValueChanged.AddListener((bool value) =>
            {
                foreach (var go in fireSmoke)
                {
                    go.SetActive(value);
                }

            });

            toggleGroundFog.onValueChanged.AddListener((bool value) =>
            {
                groundFog.SetActive(value);
            });

            toggleGroundFogLights.onValueChanged.AddListener((bool value) =>
            {
                groundFogLights.SetActive(value);
            });
            toggleGroundFogParticleLights.onValueChanged.AddListener((bool value) =>
            {
                ParticleSystem.EmissionModule emission = groundFogParticleLights.emission;
                emission.enabled = value;
            });

            groundFogAdditionalLightingSlider.value =
                groundFogRenderers[0].material.GetFloat("_AdditionalLights");

            groundFogAdditionalLightingSlider.onValueChanged.AddListener((float value) =>
            {
                foreach (var renderer in groundFogRenderers)
                {
                    renderer.material.SetFloat("_AdditionalLights", value);
                }
            });
        }

        void Update()
        {

        }
    }
}
