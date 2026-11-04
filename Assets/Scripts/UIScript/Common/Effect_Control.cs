using LetsPiu.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LetsPiu.Common.UI
{
    public class Effect_Control : MonoBehaviour
    {
        // Start is called before the first frame update
        public Vector3 mousePosition;
        public GameObject effectPredf;
        private double timer;
        public AudioSource katong;
        private GameObject lastObj;
        public Camera camera;
        void Start()
        {
            // effectPredf.GetComponent<ParticleSystem>().playOnAwake = true;
            timer = 0;
        }


        void Update()
        {
            //timer -= Time.deltaTime;
            //if (timer <= 0)
            //{
            //    playLizi();
            //    timer =2f;s
            //}

            if (Input.GetMouseButtonDown(0))
            {
                playLizi();
                //判断点击的物体是UI界面，但点击的UI不能有button和toggle的时候播放点击音效
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }
                //判断点击的物体是UI界面，但点击的UI不能有button和toggle的时候播放点击音效
                if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<Button>() != null)
                {
                    return;
                }
                if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<Toggle>() != null)
                {
                    return;
                }
                AudioPlayModule.S.PlayUIAudio(AudioDefine.UI_Click);
            }
            //Debug.LogError(timer);
        }

        void playLizi() {
            //Debug.Log("播放点击特效..");
            if (camera == null)
            {
                var cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                {
                    camera = cameras[0];
                }
            }

            if (camera == null) return;
            if (lastObj != null)
            {
                Destroy(lastObj);
            }

            effectPredf.GetComponent<ParticleSystem>().playOnAwake = true;

            mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 4f);
            mousePosition = camera.ScreenToWorldPoint(mousePosition);
            lastObj = Instantiate(effectPredf);
            var transforms = lastObj.GetComponentsInChildren<Transform>();
            foreach (var t in transforms)
            {
                t.localScale *= 0.25f;
                var particleRenderer = t.GetComponent<Renderer>();
                // 设置Sorting Layer和Order in Layer
                if (particleRenderer != null)
                {
                    particleRenderer.sortingLayerName = "Effects"; // 更改Sorting Layer名称
                    particleRenderer.sortingOrder = 100; // 设置一个较高的Order值
                }
            }
            lastObj.transform.position = mousePosition;

            Destroy(lastObj, 1);
           // AudioPlayModule.S.PlayUIAudio(AudioDefine.UI_Click);
        }


    }
}