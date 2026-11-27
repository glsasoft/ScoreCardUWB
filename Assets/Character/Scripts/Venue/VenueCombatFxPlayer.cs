using System.Collections.Generic;
using LetsPiu.Character.Uwb;
using UnityEngine;

namespace LetsPiu.Character
{
    /// <summary>
    /// 按战斗演出提示在角色/大炮挂点播放 VFX_UWB_2.5DView_* 预制体。
    /// 通过 <see cref="UwbCombatFxCatalog"/> 直接引用美术原目录资源，离线包靠引用进包，不复制特效。
    /// </summary>
    [DefaultExecutionOrder(50)]
    public class VenueCombatFxPlayer : MonoBehaviour
    {
        const string HostName = "VenueCombatFxPlayer_Host";
        const string CatalogResourcePath = "UwbCombatFxCatalog";
        const float FxLifetime = 2.5f;

        public static VenueCombatFxPlayer Current { get; private set; }

        static UwbCombatFxCatalog s_Catalog;
        readonly List<VenuePlayerView> _views = new List<VenuePlayerView>(32);
        readonly Dictionary<int, int> _lastHp = new Dictionary<int, int>(32);
        readonly Dictionary<int, BuffFx> _buffs = new Dictionary<int, BuffFx>(32);
        readonly List<int> _buffScratch = new List<int>(8);

        // 持续型 BUFF 特效：挂在角色身上一直循环，直到死亡或新一局
        sealed class BuffFx
        {
            public bool red;
            public GameObject go;
            public ParticleSystem[] systems;
        }

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
            EnsureCatalog();
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
                    ClearBuff(targetId);
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
                    ClearBuff(targetId);
                    break;
                case "Revive":
                    PlayAtPlayer(actorId, "Attack/Man/VFX_UWB_2.5DView_Attack_Reborn");
                    // Reborn 预制体只是 0.2 大小的碎块，镜头拉远后几乎看不见，叠加回血光环让复活可见
                    PlayAtPlayer(actorId, "Buff/VFX_UWB_2.5DView_BUFF_Heling");
                    TryToggleChild(actorId, "VFX_UWB_2.5DView_Attack_Dead", false);
                    TryToggleChild(actorId, "VFX_UWB_2.5DView_Attack_Reborn", true);
                    StartCoroutine(DisableChildLater(actorId, "VFX_UWB_2.5DView_Attack_Reborn", FxLifetime));
                    break;
                case "BuffRed":
                    SetBuff(actorId, true);
                    break;
                case "BuffBlue":
                    SetBuff(actorId, false);
                    break;
                case "Heal":
                    PlayAtPlayer(actorId, "Buff/VFX_UWB_2.5DView_BUFF_Heling");
                    break;
            }
        }

        public void SeedHp(int playerId, int hp)
        {
            _lastHp[playerId] = hp;
        }

        public void NotifyHp(int playerId, int hp)
        {
            // prev > 0：从 0 回升是复活，由 Revive 演出负责，不当作回血
            if (_lastHp.TryGetValue(playerId, out int prev) && prev > 0 && hp > prev)
                HandleCue("Heal", playerId, 0, 0);
            _lastHp[playerId] = hp;
        }

        /// <summary>该玩家当前持有的 BUFF：0 无，1 红，2 蓝。</summary>
        public int BuffOf(int playerId)
        {
            if (!_buffs.TryGetValue(playerId, out BuffFx fx) || fx.go == null)
                return 0;
            return fx.red ? 1 : 2;
        }

        /// <summary>死亡同帧清除；复活不恢复。</summary>
        public void ClearBuff(int playerId)
        {
            if (!_buffs.TryGetValue(playerId, out BuffFx fx))
                return;
            _buffs.Remove(playerId);
            if (fx.go != null)
                Destroy(fx.go);
        }

        public void ClearAllBuffs()
        {
            foreach (BuffFx fx in _buffs.Values)
            {
                if (fx.go != null)
                    Destroy(fx.go);
            }

            _buffs.Clear();
        }

        void SetBuff(int playerId, bool red)
        {
            if (playerId <= 0 || !TryFindView(playerId, out VenuePlayerView view))
                return;

            // 红蓝互斥：同色重复夺取只保留一份，异色则换掉
            if (_buffs.TryGetValue(playerId, out BuffFx cur) && cur.go != null && cur.red == red)
                return;
            ClearBuff(playerId);

            GameObject prefab = ResolvePrefab(red
                ? "Buff/VFX_UWB_2.5DView_BUFF_Redbuff"
                : "Buff/VFX_UWB_2.5DView_BUFF_Bluebuff");
            if (prefab == null)
                return;

            GameObject go = Instantiate(prefab);
            go.name = prefab.name + "_Loop";
            go.SetActive(true);
            go.transform.SetParent(view.transform, false);
            go.transform.localPosition = Vector3.up * 0.1f;
            go.transform.localRotation = Quaternion.identity;
            _buffs[playerId] = new BuffFx
            {
                red = red,
                go = go,
                systems = go.GetComponentsInChildren<ParticleSystem>(true),
            };
        }

        // 美术预制体的粒子都不循环，播完整套后重新播一遍，直到被清除
        void Update()
        {
            if (_buffs.Count == 0)
                return;

            _buffScratch.Clear();
            foreach (KeyValuePair<int, BuffFx> pair in _buffs)
            {
                BuffFx fx = pair.Value;
                // 角色被销毁（换人数、重建）时特效跟着没了
                if (fx.go == null)
                {
                    _buffScratch.Add(pair.Key);
                    continue;
                }

                bool alive = false;
                for (int i = 0; i < fx.systems.Length && !alive; i++)
                    alive = fx.systems[i].IsAlive(false);
                if (alive)
                    continue;
                for (int i = 0; i < fx.systems.Length; i++)
                    fx.systems[i].Play(false);
            }

            for (int i = 0; i < _buffScratch.Count; i++)
                _buffs.Remove(_buffScratch[i]);
        }

        public void ClearBodyCombatFx(int playerId)
        {
            TryToggleChild(playerId, "VFX_UWB_2.5DView_Attack_Dead", false);
            TryToggleChild(playerId, "VFX_UWB_2.5DView_Attack_Reborn", false);
        }

        System.Collections.IEnumerator DisableChildLater(int playerId, string childName, float delay)
        {
            yield return new WaitForSeconds(delay);
            TryToggleChild(playerId, childName, false);
        }

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
                xz = venueTeam == 0 ? new Vector2(8f, 8f) : new Vector2(-8f, -8f);

            var sync = VenuePlayerSync.Current;
            Vector3 venue = new Vector3(xz.x, 0f, xz.y);
            if (sync != null && sync.space != null)
                return sync.space.ToWorld(venue);
            return venue;
        }

        void SpawnFx(string relativePath, Vector3 worldPos, Transform parent, Quaternion? worldRotation)
        {
            GameObject prefab = ResolvePrefab(relativePath);
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

        static void EnsureCatalog()
        {
            if (s_Catalog != null)
                return;
            s_Catalog = Resources.Load<UwbCombatFxCatalog>(CatalogResourcePath);
            if (s_Catalog == null)
                Debug.LogError(
                    "[VenueCombatFx] 缺少 Resources/" + CatalogResourcePath +
                    ".asset（应引用 GLSA_UWB_2.5DView/Effect 原预制体）");
        }

        static GameObject ResolvePrefab(string relativePath)
        {
            EnsureCatalog();
            GameObject prefab = s_Catalog != null ? s_Catalog.Resolve(relativePath) : null;
            if (prefab == null)
                Debug.LogError("[VenueCombatFx] catalog 未绑定: " + relativePath);
            return prefab;
        }
    }
}
