using System;
using System.Security.Policy;
using Common.Network.Model.Http;
using LetsPiu.Main.Common.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu.Common.Component
{
    public class PlayerAvatarDataUI : MonoBehaviour
    {
        public Image headImage;
        public Image genderImage;
        public Sprite[] genderSprite;
        private Texture2D defaultHead;

        private string headUrl;

        public event Action<string> OnHeadImageUpdatedEvent;

        public void OnClickURL()
        {
            OnHeadImageUpdatedEvent?.Invoke(headUrl);
        }

        public void SetDefaultHeadImage(string url)
        {
            headUrl = url;
            WebImageLoader.Instance.LoadSquareSprite(headUrl, sprite =>
            {
                // 防止多次被赋值，并且防止Image已被销毁
                if (sprite != null && headImage != null && headImage.gameObject != null && headImage)
                {
                    headImage.sprite = sprite;
                }
            });
        }

        public void UpdateHeadTexture(Customer customer)
        {
            //Debug.Log(customer.sex+ "性别1");
            if (headImage != null && headImage.gameObject != null && headImage)
            {
                headUrl = customer.imageUrl;
                WebImageLoader.Instance.LoadSquareSprite(customer.imageUrl, sprite =>
                {
                    // 防止多次被赋值，并且防止Image已被销毁
                    if (sprite != null && headUrl == customer.imageUrl && headImage != null && headImage.gameObject != null && headImage)
                    {
                        headImage.sprite = sprite;
                    }
                });
            }
            
            if (customer.sex != -1)
            {
                if (genderImage == null) return;
                genderImage.sprite = genderSprite[customer.sex];
                genderImage.SetNativeSize();
            }

        }
        /// <summary>
        /// 设置头像为灰度图像（离线状态）
        /// </summary>
        public void SetHeadImageGray(bool isGray)
        {
            if (headImage == null) return;

            Material grayMat = null;
            if (isGray)
            {
                // 创建一个简单的灰度材质
                Shader shader = Shader.Find("UI/GrayScale");
                if (shader == null)
                {
                    // 如果没有自定义灰度Shader，使用内置的方式
                    grayMat = new Material(Shader.Find("UI/Default"));
                    grayMat.SetFloat("_Gray", 1f);
                }
                else
                {
                    grayMat = new Material(shader);
                }
                headImage.material = grayMat;
            }
            else
            {
                headImage.material = null;
            }
        }

        //如果玩家处于离线实例化的头像会是灰度头像，写个方法设置灰度头像
        private void OnEnable()
        {
        }
        void ODisable()
        {
        }
        void RefreshName()
        {
        }
    }
}