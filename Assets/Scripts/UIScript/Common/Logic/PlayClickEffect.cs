using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LetsPiu.Base
{
    public class PlayClickEffect : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("PlayClickEffect Start" + transform.name);
            DontDestroyOnLoad(gameObject);
        }

        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                AddEffect(Input.mousePosition);
            }
#else
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch t = Input.GetTouch(i);
                    if (t.phase == TouchPhase.Began)
                    {
                        AddEffect(t.position);
                    }
                }
            }
#endif
        }

        void AddEffect(Vector3 screenPos)
        {
            //判断点击的物体是UI界面，但点击的UI不能有button和toggle的时候播放点击音效
            //if (!EventSystem.current.IsPointerOverGameObject())
            //{
            //    return;
            //}
            //Debug.Log("是否点击UI");
            //判断点击的物体是UI界面，但点击的UI不能有button和toggle的时候播放点击音效
            if (EventSystem.current.currentSelectedGameObject != null && EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>() != null)
            {
                return;
            }

            EffectResourceModule.S.AddEffect(1207, Helper.GetEffectParent(1207), (id) =>
            {
                var effect = EffectResourceModule.S.GetEffectGameObject(id);
                if (effect)
                {
                    Debug.Log("Play Click Effect");
                    //UI坐标等于鼠标点击得位置
                    screenPos.z = 100;
                    var uiPos = UIRoot.S.uiCamera.ScreenToWorldPoint(screenPos);
                    effect.transform.position = uiPos;
                }
            });

            AudioPlayModule.S.PlayUIAudio(AudioDefine.UI_Click);
        }
    }
}


