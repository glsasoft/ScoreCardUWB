using UnityEngine;

namespace LetsPiu.Graphics
{
    public class SetSkybox : MonoBehaviour
    {
        [Header("Skybox Material")]
        public Material skyboxMaterial;
        void Start()
        {
            SkyboxManager.Instance?.SetSkybox(skyboxMaterial);
        }
    }
}
