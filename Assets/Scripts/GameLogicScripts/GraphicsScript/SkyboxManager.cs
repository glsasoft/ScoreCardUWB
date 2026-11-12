using UnityEngine;


namespace LetsPiu.Graphics
{
    public class SkyboxManager : MonoBehaviour
    {
        // 静态单例实例
        public static SkyboxManager Instance { get; private set; }

        private void Awake()
        {
            // 单例模式
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // 保证唯一性
                return;
            }
            Instance = this;
            
        }
        
        public void SetSkybox(Material newSkybox)
        {
            if (newSkybox == null)
            {
                Debug.LogWarning("SkyboxManager: 传入的材质为 null！");
                return;
            }

            RenderSettings.skybox = newSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }
}
