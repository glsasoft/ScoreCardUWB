using TMPro;
using UnityEngine;

namespace LetsPiu.Component.Common.Component
{
    /// <summary>
    /// 版本号
    /// </summary>
    public class VersionCodeInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text versionText;
        
        private void Start()
        {
            versionText.text = "版本号:"+Application.version;
        }
    }
}
