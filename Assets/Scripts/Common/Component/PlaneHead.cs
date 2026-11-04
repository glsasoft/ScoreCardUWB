using System;
using LetsPiu.Main.Common.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Component.Common.Component
{
    /// <summary>
    /// 无样式的用户头像
    /// </summary>
    public class PlaneHead : MonoBehaviour
    {
        private Image head;
        private RawImage rawImageHead;
        private string headUrl;

        private void Awake()
        {
            head = GetComponent<Image>();
            rawImageHead = GetComponent<RawImage>();
            if (headUrl != null)
            {
                LoadHeadUrl(headUrl);
            }
        }

        public void SetHeadUrl(string url)
        {
            if (headUrl == url)
                return;
            headUrl = url;
            if (gameObject.activeInHierarchy)
            {
                LoadHeadUrl(url);
            }
        }
        public void SetHeadColor(Color color)
        {
            if (head != null)
            {
                head.color = color;
            }
            if (rawImageHead != null)
            {
                rawImageHead.color = color;
            }
        }
        private void LoadHeadUrl(string targetUrl)
        {
            if (head == null && rawImageHead == null)
                return;
            if (head)
                head.enabled = false;
            if (rawImageHead)
                rawImageHead.enabled = false;
            if (head)
            {
                WebImageLoader.Instance.LoadSquareSprite(targetUrl, sprite =>
                {
                    //防止多次被赋值
                    if (head == null || targetUrl != headUrl) return;
                    head.enabled = true;
                    head.sprite = sprite;
                });
            }
            if (rawImageHead)
            {
                WebImageLoader.Instance.LoadSquareImage(targetUrl, tex =>
                {
                    if (rawImageHead == null || targetUrl != headUrl) return;
                    rawImageHead.enabled = true;
                    rawImageHead.texture = tex;
                });
            }
        }
    }
}
