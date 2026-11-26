using System.Collections.Generic;
using LetsPiu.Character.Uwb;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Character
{
    /// <summary>
    /// 按战斗演出提示在角色/大炮挂点播放 VFX_UWB_2.5DView_* 预制体。
    /// Editor 下按路径加载；正式包可把同名资源放进 Resources/UwbCombatFx。
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class VenueCombatFxPlayer : MonoBehaviour
    {
        const string HostName = "VenueCombatFxPlayer_Host";
        const string EditorRoot = "Assets/GLSA_UWB_2.5DView/Effect/";
        const float FxLifetime = 2.5f;

        public static VenueCombatFxPlayer Current { get; private set; }

        static readonly Dictionary<string, GameObject> s_Prefabs = new Dictionary<string, GameObject>(16);
        readonly List<VenuePlayerView> _views = new List<VenuePlayerView>(32);
        readonly Dictionary<int, int> _lastHp = new Dictionary<int, int>(32);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if (Current != null)
                return;
            var host = new GameObject(HostName);
            DontDestroyOnLoad(host);
            host.AddComponent<VenueCombatFxPlayer>();
        }

        void Awake()
        {
            if (Current != null && Current != this)
            {
                Destroy(gameObject);
                return;
            }

            Current = this;
        }

        void OnDestroy()
        {
            if (Current == this)
                Current = null;
        }

        public void HandleCue(
            string type,
            int actorId,
            int targetId,
            int venueTeam)
        {
            switch (type)
            {
                case "ShootHit":
                    PlayShootToward(
                        actorId,
                        targetId,
                        venueTeam == 1
                            ? "Attack/Man/VFX_UWB_2.5DView_Man_Attack_Red"
                            : "Attack/Man/VFX_UWB_2.5DView_Man_Attack_Blue");
                    PlayAtPlayer(targetId, "Attack/Man/VFX_UWB_2.5DView_Attack_Behit01");
                    break;
                case "ShootKill":
                    PlayShootToward(
                        actorId,
                        targetId,
                        venueTeam == 1
                            ? "Attack/Man/VFX_UWB_2.5DView_Man_Attack_Red"
                            : "Attack/Man/VFX_UWB_2.5DView_Man_Attack_Blue");
                    PlayAtPlayer(targetId, "Attack/Man/VFX_UWB_2.5DView_Attack_Dead");
                    TryToggleChild(targetId, "VFX_UWB_2.5DView_Attack_Dead", true);
                    break;
                case "TowerKill":
                    // venueTeam=受害者队伍：红受害者→蓝炮(右上)，蓝受害者→红炮(左下)
                    bool blueCannon = venueTeam == 1;
                    PlayAtCannon(blueCannon ? 0 : 1,
                        blueCannon
                            ? "Attack/Tower/VFX_UWB_2.5DView_Town_Bullet_Blue"
                            : "Attack/Tower/VFX_UWB_2.5DView_Town_Bullet_Red");
                    PlayAtPlayer(targetId,
                        blueCannon
                            ? "Attack/Man/VFX_UWB_2.5DView_Attack_Behit02"
                            : "Attack/Man/VFX_UWB_2.5DView_Attack_Behit02_Red");
                    PlayAtPlayer(targetId, "Attack/Man/VFX_UWB_2.5DView_Attack_Dead");
                    TryToggleChild(targetId, "VFX_UWB_2.5DView_Attack_Dead", true);
                    break;
                case "Revive":
                    PlayAtPlayer(actorId, "Attack/Man/VFX_UWB_2.5DView_Attack_Reborn");
                    TryToggleChild(actorId, "VFX_UWB_2.5DView_Attack_Dead", false);
                    TryToggleChild(actorId, "VFX_UWB_2.5DView_Attack_Reborn", true);
                    break;
                case "BuffRed":
                    PlayAtPlayer(actorId, "Buff/VFX_UWB_2.5DView_BUFF_Redbuff");
                    break;
                case "BuffBlue":
                    PlayAtPlayer(actorId, "Buff/VFX_UWB_2.5DView_BUFF_Bluebuff");
                    break;
                case "Heal":
                    PlayAtPlayer(actorId, "Buff/VFX_UWB_2.5DView_BUFF_Heling");
                    break;
            }
        }

        /// <summary>血量回升时播回血（由 MatchRuntime 在合并快照时调用）。</summary>
        public void NotifyHp(int playerId, int hp)
        {
            if (_lastHp.TryGetValue(playerId, out int prev) && hp > prev && prev >= 0)
                HandleCue("Heal", playerId, 0, 0);
            _lastHp[playerId] = hp;
        }

        /// <summary>
        /// 按 HIT/KILL 的 shooter→victim 水平朝向播枪口火光与曳光（策划：命中消息决定射击方向）。
        /// </summary>
        void PlayShootToward(int shooterId, int victimId, string attackFxPath)
        {
            if (shooterId <= 0 || !TryFindView(shooterId, out VenuePlayerView shooter))
                return;

            Vector3 aimPoint = shooter.transform.position + shooter.transform.forward * 8f;
            Quaternion? face = null;
            if (victimId > 0 && TryFindView(victimId, out VenuePlayerView victim))
            {
                aimPoint = victim.transform.position + Vector3.up * 1.2f;
                Vector3 flat = victim.transform.position - shooter.transform.position;
                flat.y = 0f;
                if (flat.sqrMagnitude > 0.0001f)
                {
                    face = Quaternion.LookRotation(flat.normalized, Vector3.up);
                    shooter.AimAt(victim.transform.position);
                }
            }

            SpawnFx(attackFxPath, shooter.transform.position, shooter.transform, face);
            shooter.FireToward(aimPoint);
        }

        void PlayAtPlayer(int playerId, string relativePath)
        {
            if (playerId <= 0 || !TryFindView(playerId, out VenuePlayerView view))
                return;
            SpawnFx(relativePath, view.transform.position, view.transform, null);
        }

        void PlayAtCannon(int venueTeam, string relativePath)
        {
            Vector3 world = ResolveCannonWorld(venueTeam);
            SpawnFx(relativePath, world, null, null);
        }

        Vector3 ResolveCannonWorld(int venueTeam)
        {
            Vector2 xz = default;
            if (UwbMockBattleHooks.GetCannonVenueXz != null)
                xz = UwbMockBattleHooks.GetCannonVenueXz(venueTeam);
            else
            {
                // 无 Mock 时用场地大致角点
                xz = venueTeam == 0 ? new Vector2(8f, 8f) : new Vector2(-8f, -8f);
            }

            var sync = VenuePlayerSync.Current;
            Vector3 venue = new Vector3(xz.x, 0f, xz.y);
            if (sync != null && sync.space != null)
                return sync.space.ToWorld(venue);
            return venue;
        }

        void SpawnFx(string relativePath, Vector3 worldPos, Transform parent, Quaternion? worldRotation)
        {
            GameObject prefab = LoadPrefab(relativePath);
            if (prefab == null)
                return;

            GameObject go = Instantiate(prefab);
            go.name = prefab.name + "_Play";
            go.SetActive(true);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.localPosition = Vector3.up * 0.1f;
                if (worldRotation.HasValue)
                    go.transform.rotation = worldRotation.Value;
                else
                    go.transform.localRotation = Quaternion.identity;
            }
            else
            {
                go.transform.position = worldPos;
                if (worldRotation.HasValue)
                    go.transform.rotation = worldRotation.Value;
            }

            Destroy(go, FxLifetime);
        }

        void TryToggleChild(int playerId, string childName, bool active)
        {
            if (!TryFindView(playerId, out VenuePlayerView view))
                return;
            Transform t = view.transform.Find(childName);
            if (t == null)
            {
                foreach (Transform c in view.GetComponentsInChildren<Transform>(true))
                {
                    if (c.name == childName)
                    {
                        t = c;
                        break;
                    }
                }
            }

            if (t != null)
                t.gameObject.SetActive(active);
        }

        bool TryFindView(int playerId, out VenuePlayerView view)
        {
            view = null;
            var sync = VenuePlayerSync.Current;
            if (sync == null)
                return false;
            _views.Clear();
            sync.CollectViews(_views);
            for (int i = 0; i < _views.Count; i++)
            {
                if (_views[i] != null && _views[i].Id == playerId)
                {
                    view = _views[i];
                    return true;
                }
            }

            return false;
        }

        static GameObject LoadPrefab(string relativePath)
        {
            if (s_Prefabs.TryGetValue(relativePath, out GameObject cached) && cached != null)
                return cached;

            string fileName = relativePath;
            int slash = relativePath.LastIndexOf('/');
            if (slash >= 0)
                fileName = relativePath.Substring(slash + 1);

            GameObject prefab = Resources.Load<GameObject>("UwbCombatFx/" + fileName);
#if UNITY_EDITOR
            if (prefab == null)
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EditorRoot + relativePath + ".prefab");
#endif
            if (prefab != null)
                s_Prefabs[relativePath] = prefab;
            else
                Debug.LogWarning("[VenueCombatFx] missing prefab " + relativePath);
            return prefab;
        }
    }
}
