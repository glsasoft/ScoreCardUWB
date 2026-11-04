using UnityEngine;

namespace LetsPiu.UI.UIScript.WebViewScripts.Test
{
    public class TitleWebViewSamples : MonoBehaviour
    {

        public TitleWebView titleWebView;
        
        // Start is called before the first frame update
        void Start()
        {
            titleWebView.LoadUrl("测试", "https://www.letspiu.com/privacyPhone.html");
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
