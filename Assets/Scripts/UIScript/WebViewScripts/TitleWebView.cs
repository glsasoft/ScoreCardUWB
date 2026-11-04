using System;
using TMPro;
using UnityEngine;

namespace LetsPiu.UI.UIScript.WebViewScripts
{
    /// <summary>
    /// 带标题的webview窗口
    /// </summary>
    public class TitleWebView : MonoBehaviour
    {
        
        public event Action WebViewCloseEvent;
        public event Action WebViewPageLoadEvent;
        public TMP_Text titleText;
        public CompactWebView compactWebView;

        private void Awake()
        {
            compactWebView.PageLoadEvent += WebViewPageLoadEvent;
        }

        public void LoadUrl(string title, string url)
        {
            gameObject.SetActive(true);
            Debug.Log("TitleWebView LoadUrl:"+url+" title="+title);
            titleText.text = title;
            compactWebView.LoadUrl(url);
        }

        public void CloseWebView()
        {
            compactWebView.DestroyBrowser();
            gameObject.SetActive(false);
            WebViewCloseEvent?.Invoke();
        }
    }
}
