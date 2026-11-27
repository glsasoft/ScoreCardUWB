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
        static readonly int MoveZId = Animator.StringToHash("MoveZ");
        static readonly int MoveSpeedId = Animator.StringToHash("MoveSpeed");
        // 混合树里 (0,2) 是跑步，(0,1) 是走路；固定取跑步
        const float RunBlendZ = 2f;
        // Move 状态的播放倍率。跑步片段一圈 0.667 秒，混合树里又乘了 0.5，倍率 1 时每秒只有 0.75 圈，看起来像贴地滑行；
        // 取 3 约每秒 2.25 圈，接近正常跑步步频，也是改动前 Mock 平均移速下的实际效果
        const float RunPlaybackSpeed = 3f;
        const float ShootHoldSec = 0.3f;
        const float DieMinSec = 1f;
        const float RunStartRatio = 0.15f;
        const float RunStopRatio = 0.08f;
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

        bool _hasMoveZ;
        bool _hasMoveSpeed;
        float _lastMoveZ;
        float _lastMoveSpeed;
        int _dieState;
        int _idleState;
        int _shootState;
        int _shootRunState;
        int _reviveState;
        float _shootUntil;
        bool _dead;
        bool _dieShown;
        float _dieLockUntil;
        bool _moving;
        int _team = int.MinValue;
        int _shownHp = int.MinValue;
        int _shownMax = int.MinValue;
        string _name = "";
        float _gaitReferenceSpeed = 1.8f;
        float _tracerUntil;
        float _aimUntil;
        Quaternion _aimRot = Quaternion.identity;

        Vector3 _velocity = Vector3.zero;
        bool _hasPose;
        int _hp;
        int _maxHp;

        public int Id { get; private set; }
        public int Team => _team;
        public string DisplayName => _name;
        public int Hp => _hp;
        public int MaxHp => _maxHp;

        /// <summary>头顶标牌挂点的世界坐标（脚底上方 statusHeight）。</summary>
        public Vector3 HeadWorldPosition => _status != null ? _status.position : transform.position;

        /// <summary>世界空间头顶血条是否显示。UI HUD 接管头顶标牌时关掉。</summary>
        public bool StatusVisible
        {
            get => _status != null && _status.gameObject.activeSelf;
            set
            {
                if (_status != null && _status.gameObject.activeSelf != value)
                    _status.gameObject.SetActive(value);
            }
        }

        public void Bind(int id, Renderer tintTarget, float gaitReferenceSpeed, float statusHeight, float worldUiScale)
        {
            Id = id;
            name = "Player_" + id;
            _tintTarget = tintTarget;
            _gaitReferenceSpeed = Mathf.Max(0.1f, gaitReferenceSpeed);
            _animator = GetComponentInChildren<Animator>();
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                _hasMoveZ = HasParameter("MoveZ");
                _hasMoveSpeed = HasParameter("MoveSpeed");
                _dieState = FindState("Die");
                _idleState = FindState("Idle");
                _shootState = FindState("Shoot");
                _shootRunState = FindState("ShootRun");
                _reviveState = FindState("Revive");
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
            //BuildTracer();
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
                if (_fillImage) _fillImage.color = color;
            }
            _shownHp = int.MinValue;
        }

        public void Show(Vector3 worldPos, Quaternion worldRot, int hp, int maxHp)
        {
            _hp = hp;
            _maxHp = maxHp;
            float dt = Time.deltaTime;
            Vector3 delta = worldPos - transform.position;
            delta.y = 0f;
            // 刚创建时物体还在出生点，这一下是摆到场上，不是在跑。拿这段距离除以帧时间会得到很大的速度，并且要衰减一会儿。
            if (!_hasPose || dt < 0.0001f)
            {
                _hasPose = true;
                _velocity = Vector3.zero;
            }
            else
            {
                Vector3 instant = delta / dt;
                _velocity = Vector3.Lerp(instant, _velocity, Mathf.Exp(-4f * dt));
            }

            // HIT/KILL 时短暂朝向受害者；覆盖期内不用位移朝向盖掉射击朝向。
            Quaternion rot = Time.time < _aimUntil ? _aimRot : worldRot;
            transform.SetPositionAndRotation(worldPos, rot);

            Vector3 localVelocity = Quaternion.Inverse(rot) * _velocity;
            bool dead = hp <= 0;
            ApplyAnimator(localVelocity, dead);
            ApplyStatus(hp, maxHp, dead);

            if (_tintTarget != null && _animator == null)
                _tintTarget.transform.localRotation = dead ? Quaternion.Euler(80f, 0f, 0f) : Quaternion.identity;
        }

        /// <summary>按击中目标水平朝向转向（策划：命中消息决定射击朝向）。</summary>
        public void AimAt(Vector3 worldTarget, float holdSec = 0.28f)
        {
            Vector3 dir = worldTarget - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                return;
            _aimRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
            _aimUntil = Time.time + Mathf.Max(0.05f, holdSec);
            transform.rotation = _aimRot;
        }

        public void Fire()
        {
            FireToward(transform.position + transform.forward * 18f);
        }

        /// <summary>曳光朝向目标点（通常为被击中玩家）。</summary>
        public void FireToward(Vector3 worldTarget)
        {
            PlayShootAnimation();
            if (_tracer == null)
                return;
            Vector3 origin = transform.TransformPoint(new Vector3(0f, 1.35f, 0.25f));
            Vector3 dir = worldTarget - origin;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f)
                dir = transform.forward;
            else
                dir.Normalize();
            _tracer.SetPosition(0, origin);
            _tracer.SetPosition(1, origin + dir * 18f);
            _tracer.enabled = true;
            _tracerUntil = Time.time + 0.12f;
        }

        // 射击动作是全身动作：在跑用小跑射击，否则用站立射击；到时间由 ApplyAnimator 切回 Idle，再由 MoveSpeed 转进 Move。
        void PlayShootAnimation()
        {
            if (_animator == null || _dead)
                return;
            int state = _moving && _shootRunState != 0 ? _shootRunState : _shootState;
            if (state == 0)
                return;
            _animator.CrossFade(state, 0.05f, 0);
            _shootUntil = Time.time + ShootHoldSec;
        }

        //void LateUpdate()
        //{
        //    Camera cam = Camera.main;
        //    if (cam != null && _status != null)
        //    {
        //        //Vector3 toCam = cam.transform.position - _status.position;
        //        //if (toCam.sqrMagnitude > 0.001f)
        //        //    _status.rotation = Quaternion.LookRotation(-toCam, Vector3.up);
        //        _status.transform.forward = cam.transform.forward;
        //    }

        //    if (_tracer != null && _tracer.enabled && Time.time >= _tracerUntil)
        //        _tracer.enabled = false;
        //}

        void OnDestroy()
        {
            // Android LeftPanel 优化可能把 Status 拆到 Canvas 下，需单独销毁
            if (_status != null)
                Destroy(_status.gameObject);

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
                if (!_dead)
                {
                    _dieLockUntil = Time.time + DieMinSec;
                    if (_dieState != 0)
                    {
                        _animator.CrossFade(_dieState, 0.1f, 0);
                        _dieShown = true;
                    }
                }
                _dead = true;
                _shootUntil = 0f;

                // 阵亡后走回复活点：倒地动作播完后，在走就切回走路，停下再回到倒地
                float deadSpeed = localVelocity.magnitude;
                _moving = deadSpeed > _gaitReferenceSpeed * (_moving ? RunStopRatio : RunStartRatio);
                bool walk = _moving && Time.time >= _dieLockUntil;
                if (walk && _dieShown && _idleState != 0)
                {
                    _animator.CrossFade(_idleState, 0.1f, 0);
                    _dieShown = false;
                }
                else if (!_moving && !_dieShown && _dieState != 0)
                {
                    _animator.CrossFade(_dieState, 0.1f, 0);
                    _dieShown = true;
                }

                float deadMoveZ = walk ? RunBlendZ : 0f;
                float deadMoveSpeed = walk ? RunPlaybackSpeed : 0f;
                if (_hasMoveZ && !Mathf.Approximately(deadMoveZ, _lastMoveZ)) _animator.SetFloat(MoveZId, deadMoveZ);
                if (_hasMoveSpeed && !Mathf.Approximately(deadMoveSpeed, _lastMoveSpeed)) _animator.SetFloat(MoveSpeedId, deadMoveSpeed);
                _lastMoveZ = deadMoveZ;
                _lastMoveSpeed = deadMoveSpeed;
                return;
            }

            _dieShown = false;
            if (_dead)
            {
                int reviveTarget = _reviveState != 0 ? _reviveState : _idleState;
                if (reviveTarget != 0)
                    _animator.CrossFade(reviveTarget, 0.1f, 0);
            }
            else if (_shootUntil > 0f && Time.time >= _shootUntil)
            {
                _shootUntil = 0f;
                if (_idleState != 0)
                    _animator.CrossFade(_idleState, 0.1f, 0);
            }
            _dead = false;

            // 只播一种跑步，不随速度改变步频：用门槛判断「在跑/不在跑」，在跑时参数固定。
            // 起跑门槛高于停下门槛，避免定位抖动让角色在原地忽跑忽停。
            float speed = localVelocity.magnitude;
            _moving = speed > _gaitReferenceSpeed * (_moving ? RunStopRatio : RunStartRatio);

            float moveZ = _moving ? RunBlendZ : 0f;
            float moveSpeed = _moving ? RunPlaybackSpeed : 0f;

            if (_hasMoveZ && !Mathf.Approximately(moveZ, _lastMoveZ)) _animator.SetFloat(MoveZId, moveZ);
            if (_hasMoveSpeed && !Mathf.Approximately(moveSpeed, _lastMoveSpeed)) _animator.SetFloat(MoveSpeedId, moveSpeed);

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
            return;

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
