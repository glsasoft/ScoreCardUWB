using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace LetsPiu.Character
{
    /// <summary>
    /// 一个玩家在场景里的样子：位移、朝向、走跑、射击、血条、死亡。
    /// 由 VenuePlayerSync 创建，不需要手工挂到场景里。
    /// </summary>
    [DisallowMultipleComponent]
    public class VenuePlayerView : MonoBehaviour
    {
        static readonly int MoveXId = Animator.StringToHash("MoveX");
        static readonly int MoveZId = Animator.StringToHash("MoveZ");
        static readonly int MoveSpeedId = Animator.StringToHash("MoveSpeed");
        static readonly float AnimationWalkSpeed = 1f;
        static readonly float AnimationRunSpeed = 2f;
        static readonly Color TeamBlue = new Color(0.25f, 0.55f, 1f);
        static readonly Color TeamRed = new Color(1f, 0.35f, 0.3f);

        Animator _animator;
        Renderer _tintTarget;
        Transform _status;
        TextMeshProUGUI _label;
        LineRenderer _tracer;
        Material _bodyMaterial;
        Image _fillImage;
        Material _tracerMaterial;
        readonly List<Material> _owned = new List<Material>(4);

        bool _hasMoveX;
        bool _hasMoveZ;
        bool _hasMoveSpeed;
        float _lastMoveX;
        float _lastMoveZ;
        float _lastMoveSpeed;
        int _dieState;
        int _idleState;
        bool _dead;
        int _team = int.MinValue;
        int _shownHp = int.MinValue;
        int _shownMax = int.MinValue;
        string _name = "";
        float _gaitReferenceSpeed = 1.8f;
        float _tracerUntil;

        Vector3 _velocity = Vector3.zero;

        public void Bind(int id, Renderer tintTarget, float gaitReferenceSpeed, float statusHeight, float worldUiScale)
        {
            name = "Player_" + id;
            _tintTarget = tintTarget;
            _gaitReferenceSpeed = Mathf.Max(0.1f, gaitReferenceSpeed);
            _animator = GetComponentInChildren<Animator>();
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                _hasMoveX = HasParameter("MoveX");
                _hasMoveZ = HasParameter("MoveZ");
                _hasMoveSpeed = HasParameter("MoveSpeed");
                _dieState = FindState("Die");
                _idleState = FindState("Idle");
            }

            if (_tintTarget != null)
            {
                _bodyMaterial = CreateUnlit(TeamBlue);
                if (_bodyMaterial != null)
                {
                    _tintTarget.sharedMaterial = _bodyMaterial;
                    _owned.Add(_bodyMaterial);
                }
            }

            BuildStatus(statusHeight, worldUiScale);
            BuildTracer();
        }

        public void SetIdentity(string playerName, int team)
        {
            _name = string.IsNullOrEmpty(playerName) ? name : playerName;
            if (_team != team)
            {
                _team = team;
                Color color = team == 0 ? TeamBlue : TeamRed;
                SetColor(_bodyMaterial, color);
                SetColor(_tracerMaterial, color);
                _fillImage.color = color;
            }
            _shownHp = int.MinValue;
        }

        public void Show(Vector3 worldPos, Quaternion worldRot, int hp, int maxHp)
        {
            float dt = Time.deltaTime;
            Vector3 delta = worldPos - transform.position;
            delta.y = 0f;
            _velocity = Vector3.Lerp(delta / dt, _velocity, Mathf.Exp(-4 * dt));
            Vector3 localVelocity = Quaternion.Inverse(worldRot) * _velocity;

            transform.SetPositionAndRotation(worldPos, worldRot);

            bool dead = hp <= 0;
            ApplyAnimator(localVelocity, dead);
            ApplyStatus(hp, maxHp, dead);
            if (_tintTarget != null && _animator == null)
                _tintTarget.transform.localRotation = dead ? Quaternion.Euler(80f, 0f, 0f) : Quaternion.identity;
        }

        public void Fire()
        {
            if (_tracer == null)
                return;
            Vector3 origin = transform.TransformPoint(new Vector3(0f, 1.35f, 0.25f));
            _tracer.SetPosition(0, origin);
            _tracer.SetPosition(1, origin + transform.forward * 18f);
            _tracer.enabled = true;
            _tracerUntil = Time.time + 0.12f;
        }

        void LateUpdate()
        {
            Camera cam = Camera.main;
            if (cam != null && _status != null)
            {
                //Vector3 toCam = cam.transform.position - _status.position;
                //if (toCam.sqrMagnitude > 0.001f)
                //    _status.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
                _status.transform.forward = cam.transform.forward;
            }

            if (_tracer != null && _tracer.enabled && Time.time >= _tracerUntil)
                _tracer.enabled = false;
        }

        void OnDestroy()
        {
            for (int i = 0; i < _owned.Count; i++)
            {
                if (_owned[i] != null)
                    Destroy(_owned[i]);
            }
        }

        void ApplyAnimator(Vector3 localVelocity, bool dead)
        {
            if (_animator == null)
                return;

            if (dead)
            {
                if (!_dead && _dieState != 0)
                    _animator.CrossFade(_dieState, 0.1f, 0);
                _dead = true;
                if (_hasMoveSpeed && !Mathf.Approximately(0, _lastMoveSpeed))
                    _animator.SetFloat(MoveSpeedId, 0f);
                _lastMoveSpeed = 0f;
                return;
            }

            if (_dead && _idleState != 0)
                _animator.CrossFade(_idleState, 0.1f, 0);
            _dead = false;

            float speed = localVelocity.magnitude;
            Vector3 aniVelocity = Vector3.zero;
            if (speed < 0.01f)
                speed = 0f;
            else
                aniVelocity = localVelocity / _gaitReferenceSpeed;

            //float t = Mathf.Exp(-4 * Time.deltaTime);
            //float moveX = Mathf.Lerp(Mathf.Clamp(aniVelocity.x, -AnimationRunSpeed, AnimationRunSpeed), _lastMoveX, t);
            //float moveZ = Mathf.Lerp(Mathf.Clamp(aniVelocity.z, -AnimationWalkSpeed, AnimationRunSpeed), _lastMoveZ, t);
            //float moveSpeed = Mathf.Lerp(speed / _animator.transform.lossyScale.x, _lastMoveSpeed, t);

            float moveX = Mathf.Clamp(aniVelocity.x, -AnimationRunSpeed, AnimationRunSpeed);
            float moveZ = Mathf.Clamp(aniVelocity.z, -AnimationWalkSpeed, AnimationRunSpeed);
            float moveSpeed = speed / _animator.transform.lossyScale.x;

            //if (_hasMoveX && !Mathf.Approximately(moveX, _lastMoveX)) _animator.SetFloat(MoveXId, moveX);
            if (_hasMoveZ && !Mathf.Approximately(moveZ, _lastMoveZ)) _animator.SetFloat(MoveZId, moveZ);
            if (_hasMoveSpeed && !Mathf.Approximately(moveSpeed, _lastMoveSpeed)) _animator.SetFloat(MoveSpeedId, moveSpeed);

            _lastMoveX = moveX;
            _lastMoveZ = moveZ;
            _lastMoveSpeed = moveSpeed;
        }

        void ApplyStatus(int hp, int maxHp, bool dead)
        {
            if (hp == _shownHp && maxHp == _shownMax)
                return;
            _shownHp = hp;
            _shownMax = maxHp;
            float ratio = maxHp > 0 ? Mathf.Clamp01(hp / (float)maxHp) : 0f;
            if (_fillImage != null)
            {
                _fillImage.rectTransform.sizeDelta = new Vector2(100 * ratio, 10);
            }
            if (_label != null)
                _label.text = dead ? _name + "  阵亡" : _name + "  " + Mathf.Max(0, hp);
        }

        void BuildStatus(float height, float worldUiScale)
        {
            var root = new GameObject("Status");
            _status = root.transform;
            _status.SetParent(transform, false);
            _status.localPosition = new Vector3(0f, height, 0f);
            _status.localScale = Vector3.one * worldUiScale / transform.lossyScale.x;

            var back = new GameObject("HpBack", typeof(RectTransform));
            back.AddComponent<Image>().color = Color.gray;
            back.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 10);
            back.transform.SetParent(_status, false);

            var fill = new GameObject("HpFill", typeof(RectTransform));
            _fillImage = fill.AddComponent<Image>();
            _fillImage.color = TeamBlue;
            _fillImage.transform.SetParent(back.transform, false);
            var fillRectTrans = _fillImage.GetComponent<RectTransform>();
            fillRectTrans.sizeDelta = new Vector2(100, 10);
            fillRectTrans.anchorMin = new Vector2(0f, 0.5f);
            fillRectTrans.anchorMax = new Vector2(0f, 0.5f);
            fillRectTrans.pivot = new Vector2(0f, 0.5f);
            fillRectTrans.anchoredPosition = Vector2.zero;

            var textGo = new GameObject("Name", typeof(RectTransform));
            textGo.transform.SetParent(_status, false);
            var textRectTrans = textGo.GetComponent<RectTransform>();
            textRectTrans.sizeDelta = new Vector2(200, 40);
            textRectTrans.anchoredPosition = new Vector2(0, 30);
            _label = textGo.AddComponent<TextMeshProUGUI>();
            _label.fontSize = 24;
            _label.alignment = TextAlignmentOptions.Center;
            _label.enableWordWrapping = false;
            _label.color = Color.white;
            _label.text = _name;
        }

        void BuildTracer()
        {
            var go = new GameObject("Tracer");
            go.transform.SetParent(transform, false);
            _tracer = go.AddComponent<LineRenderer>();
            _tracer.positionCount = 2;
            _tracer.startWidth = 0.04f;
            _tracer.endWidth = 0.01f;
            _tracer.useWorldSpace = true;
            _tracer.shadowCastingMode = ShadowCastingMode.Off;
            _tracer.receiveShadows = false;
            _tracerMaterial = CreateUnlit(TeamBlue);
            if (_tracerMaterial != null)
            {
                _tracer.sharedMaterial = _tracerMaterial;
                _owned.Add(_tracerMaterial);
            }
            _tracer.enabled = false;
        }

        bool HasParameter(string parameterName)
        {
            AnimatorControllerParameter[] parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].name == parameterName)
                    return true;
            }
            return false;
        }

        int FindState(string stateName)
        {
            int hash = Animator.StringToHash(stateName);
            if (_animator.HasState(0, hash))
                return hash;
            hash = Animator.StringToHash("Base Layer." + stateName);
            if (_animator.HasState(0, hash))
                return hash;
            return 0;
        }

        static Font BuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return font;
        }

        static Material CreateUnlit(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");
            if (shader == null)
                return null;
            var mat = new Material(shader);
            SetColor(mat, color);
            return mat;
        }

        static void SetColor(Material mat, Color color)
        {
            if (mat == null)
                return;
            mat.color = color;
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
        }
    }
}
