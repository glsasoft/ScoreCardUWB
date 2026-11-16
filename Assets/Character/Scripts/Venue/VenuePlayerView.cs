using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        static readonly Color TeamBlue = new Color(0.25f, 0.55f, 1f);
        static readonly Color TeamRed = new Color(1f, 0.35f, 0.3f);

        Animator _animator;
        Renderer _tintTarget;
        Transform _status;
        Transform _fill;
        TextMesh _label;
        LineRenderer _tracer;
        Material _bodyMaterial;
        Material _fillMaterial;
        Material _tracerMaterial;
        readonly List<Material> _owned = new List<Material>(4);

        bool _hasMoveX;
        bool _hasMoveZ;
        bool _hasMoveSpeed;
        float _lastMoveX = float.NaN;
        float _lastMoveZ = float.NaN;
        float _lastMoveSpeed = float.NaN;
        int _dieState;
        int _idleState;
        bool _dead;
        bool _hasPose;
        int _team = int.MinValue;
        int _shownHp = int.MinValue;
        int _shownMax = int.MinValue;
        string _name = "";
        float _gaitReferenceSpeed = 1.8f;
        float _teleportDistance = 8f;
        float _tracerUntil;

        Vector3 _velocity = Vector3.zero;

        public void Bind(int id, Renderer tintTarget, float gaitReferenceSpeed, float teleportDistance, float statusHeight)
        {
            name = "Player_" + id;
            _tintTarget = tintTarget;
            _gaitReferenceSpeed = Mathf.Max(0.1f, gaitReferenceSpeed);
            _teleportDistance = teleportDistance;
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

            BuildStatus(statusHeight);
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
                SetColor(_fillMaterial, color);
                SetColor(_tracerMaterial, color);
            }
            _shownHp = int.MinValue;
        }

        public void Show(Vector3 worldPos, Quaternion worldRot, int hp, int maxHp)
        {
            float dt = Time.deltaTime;
            Vector3 delta = worldPos - transform.position;
            delta.y = 0f;
            Vector3 targetVelocity = dt > 0.0001f ? delta / dt : Vector3.zero;
            _velocity = Vector3.Lerp(targetVelocity, _velocity, Mathf.Exp(-4 * dt));
            bool teleport = !_hasPose || delta.sqrMagnitude > _teleportDistance * _teleportDistance;
            float speed = 0f;
            Vector3 local = Vector3.zero;
            if (!teleport)
            {
                speed = _velocity.magnitude;
                local = Quaternion.Inverse(worldRot) * _velocity;
            }

            transform.SetPositionAndRotation(worldPos, worldRot);
            _hasPose = true;

            bool dead = hp <= 0;
            ApplyAnimator(speed, local, dead);
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
                Vector3 toCam = cam.transform.position - _status.position;
                if (toCam.sqrMagnitude > 0.001f)
                    _status.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
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

        void ApplyAnimator(float speed, Vector3 local, bool dead)
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
                _animator.CrossFade(_idleState, 0.15f, 0);
            _dead = false;

            if (speed < 0.15f)
            {
                //if (_hasMoveX && !Mathf.Approximately(0, _lastMoveX)) _animator.SetFloat(MoveXId, 0f);
                if (_hasMoveZ && !Mathf.Approximately(0, _lastMoveZ)) _animator.SetFloat(MoveZId, 0f);
                if (_hasMoveSpeed && !Mathf.Approximately(0, _lastMoveSpeed)) _animator.SetFloat(MoveSpeedId, 0f);
                _lastMoveX = 0f;
                _lastMoveZ = 0f;
                _lastMoveSpeed = 0f;
                return;
            }

            float moveX = Mathf.Clamp(local.x / _gaitReferenceSpeed, -2, 2);
            float moveZ = Mathf.Clamp(local.z / _gaitReferenceSpeed, -1, 2);
            float moveSpeed = speed / this.transform.localScale.x;

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
            if (_fill != null)
            {
                _fill.localScale = new Vector3(ratio, 1f, 1f);
                _fill.localPosition = new Vector3((ratio - 1f) * 0.5f, 0f, 0.02f);
            }
            if (_label != null)
                _label.text = dead ? _name + "  阵亡" : _name + "  " + Mathf.Max(0, hp);
        }

        void BuildStatus(float height)
        {
            var root = new GameObject("Status");
            _status = root.transform;
            _status.SetParent(transform, false);
            _status.localPosition = new Vector3(0f, height, 0f);

            var back = MakeQuad("HpBack", new Color(0f, 0f, 0f, 0.55f), new Vector3(1.2f, 0.16f, 1f), out _);
            back.SetParent(_status, false);

            _fill = MakeQuad("HpFill", TeamBlue, Vector3.one, out _fillMaterial);
            _fill.SetParent(back, false);

            var textGo = new GameObject("Name");
            textGo.transform.SetParent(_status, false);
            textGo.transform.localPosition = new Vector3(0f, 0.28f, 0.02f);
            _label = textGo.AddComponent<TextMesh>();
            _label.font = BuiltinFont();
            _label.fontSize = 64;
            _label.characterSize = 0.35f;
            _label.anchor = TextAnchor.MiddleCenter;
            _label.alignment = TextAlignment.Center;
            _label.color = Color.white;
            _label.text = _name;
        }

        Transform MakeQuad(string quadName, Color color, Vector3 scale, out Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = quadName;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            go.transform.localScale = scale;
            var renderer = go.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            mat = CreateUnlit(color);
            if (mat != null)
            {
                renderer.sharedMaterial = mat;
                _owned.Add(mat);
            }
            return go.transform;
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
