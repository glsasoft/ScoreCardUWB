using Cysharp.Threading.Tasks;
using LetsPiu.Base;
using LetsPiu.Main;
using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace LetsPiu
{
    public class AvatarShow : MonoBehaviour
    {
        [SerializeField]
        Transform m_3DRoot;
        [SerializeField]
        Camera m_Camera;
        [SerializeField]
        RawImage m_Image;
        [SerializeField]
        Light m_Light;
        [SerializeField]
        Color m_BgColor = new(0.4f, 0.4f, 0.4f, 0);

        RenderTexture m_RendererTexture;
        string m_AvatarName;
        GameObject m_Obj;
        Rect m_ImageRTOrignalSize;

        Vector3 m_ObjPos = new(0, 0, 0);
        Vector3 m_ObjRot = new(0, 180, 0);
        string m_ObjAnimaName = string.Empty;

        public async UniTask SetConfig(string avatarName)
        {
            if (string.IsNullOrEmpty(avatarName))
            {
                return;
            }

            if (avatarName == m_AvatarName)
            {
                return;
            }
            if (m_Obj != null)
            {
                SetPhysical(true);
                // 不要直接删除，重置给管理器
                AvatarMgr.S.RecycleAvater(m_AvatarName, m_Obj);
            }
            m_AvatarName = avatarName;

            m_Obj = await AvatarMgr.S.GetAvatar(m_AvatarName);
            var loadGroup = m_Obj.GetComponentInChildren<LODGroup>();
            if (loadGroup != null)
            {
                loadGroup.ForceLOD(0);
            }
            SetAvatarProp();
        }

        // 替换 SetAvatarAnimAndDrag 方法
        public void SetAvatarAnimAndDrag()
        {
            if (m_Obj == null)
            {
                return;
            }
            float x = Input.GetAxis("Mouse X");
            m_Obj.transform.Rotate(Vector3.up, -x * 5f);
        }

        public void SetPhysical(bool canUse)
        {
            var rb = m_Obj.GetComponentInChildren<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = canUse;
                rb.isKinematic = !canUse;
            }
        }

        //根据传入动画名字设置模型动画并播放
        public void SetAvatarAnim(string animName)
        {
            m_ObjAnimaName = animName;
            if (m_Obj != null)
            {
                if (m_Obj.TryGetComponent<Animator>(out var animator))
                {
                    animator.Play(animName);
                }
            }
        }

        public void SetPosAndRot(Vector3 pos, Vector3 rot)
        {
            m_ObjPos = pos;
            m_ObjRot = rot;
            if (m_Obj != null)
            {
                var tran = m_Obj.transform;
                tran.localPosition = Vector3.zero;
                tran.SetLocalPositionAndRotation(pos, Quaternion.Euler(rot));
            }
        }

        void Awake()
        {
            m_ImageRTOrignalSize = m_Image.gameObject.GetComponent<RectTransform>().rect;

            Init3DTransform();

            InitRealTimeLight();
            InitTextureScale();
        }

        void OnDestroy()
        {
            UnInitRealTimeLight();
            if (m_Obj != null)
            {
                var loadGroup = m_Obj.GetComponentInChildren<LODGroup>();
                if (loadGroup != null)
                {
                    loadGroup.ForceLOD(-1);
                }
                // 重置给管理器
                AvatarMgr.S.RecycleAvater(m_AvatarName, m_Obj);
            }

            if (m_RendererTexture != null)
            {
                RenderTexture.ReleaseTemporary(m_RendererTexture);
                m_RendererTexture = null;
            }

            if (m_3DRoot != null)
            {
                Destroy(m_3DRoot.gameObject);
                m_3DRoot = null;
            }
        }

        void OnEnable()
        {
            EnableCamera(true);
            EnableLight(false);
        }

        void OnDisable()
        {
            EnableCamera(false);
            EnableLight(false);
        }

        void EnableCamera(bool enable)
        {
            if (m_Camera != null)
            {
                m_Camera.gameObject.SetActive(enable);
                m_Camera.backgroundColor = m_BgColor;
                m_Camera.clearFlags = CameraClearFlags.SolidColor;
            }
        }

        void EnableLight(bool enable)
        {
            if (m_Light != null)
            {
                m_Light.enabled = enable;
            }
        }

        void Init3DTransform()
        {
            if (m_3DRoot != null)
            {
                AvatarShowMgr.S.SetAvatarRootParent(m_3DRoot);
            }
        }

        void InitRealTimeLight()
        {
            // 独立光源
            if (m_Light != null)
            {
                return;
                m_Light.cullingMask = 1 << Layer.AVATAR_SHOW;
                m_Light.enabled = false;

                AvatarShowMgr.S.enableLight = false;
                m_Light.enabled = true;
            }
        }

        void UnInitRealTimeLight()
        {
            if (m_Light != null)
            {
                AvatarShowMgr.S.enableLight = true;
                m_Light.enabled = false;
            }
        }

        void InitTextureScale()
        {
            if (m_RendererTexture != null)
            {
                RenderTexture.ReleaseTemporary(m_RendererTexture);
                m_RendererTexture = null;
            }

            m_RendererTexture = RenderTexture.GetTemporary((int)m_ImageRTOrignalSize.width, (int)m_ImageRTOrignalSize.height, 24, GraphicsFormat.R8G8B8A8_UNorm, 4);
            // 关闭MSAA
            m_RendererTexture.antiAliasing = 1;
            if (m_Camera != null)
            {
                m_Camera.targetTexture = m_RendererTexture;
            }

            if (m_Image != null)
            {
                m_Image.texture = m_RendererTexture;
                m_Image.enabled = true;
            }
        }

        void SetAvatarProp()
        {
            m_Obj.SetActive(true);
            UITools.SetGameObjectLayer(m_Obj, Layer.AVATAR_SHOW);
            Helper.SetParent(m_Obj, m_3DRoot);
            SetPosAndRot(m_ObjPos, m_ObjRot);
            SetAvatarAnim(m_ObjAnimaName);
        }
    }
}
