using System.Collections.Generic;
using LetsPiu.Character;
using LetsPiu.Character.Uwb;
using UnityEngine;

namespace LetsPiu.UI
{
    /// <summary>
    /// UWB 单屏比分牌「游戏中」HUD 面板：顶部比分条、任务说明、底部队员卡片、人物头顶标牌。
    /// 人物位置、血量取自场上的 VenuePlayerSync；击杀死亡、比分、倒计时取自 UwbHudData。
    /// 地图和人物是 3D 场景，不属于本面板。
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class UwbGameHudPanel : AbstractPanel
    {
        const int SlotsPerTeam = 5;

        [SerializeField] UwbHudTopBar topBar;
        [SerializeField] RectTransform tagLayer;
        [SerializeField] UwbPlayerTagItem tagTemplate;
        [SerializeField] UwbPlayerCardItem[] redCards;
        [SerializeField] UwbPlayerCardItem[] blueCards;

        [Tooltip("标牌底边中点相对人物头顶挂点的偏移（画布单位，向上为正）")]
        [SerializeField] Vector2 tagOffset = new Vector2(0f, 6f);

        readonly List<VenuePlayerView> m_Views = new List<VenuePlayerView>(32);
        readonly Dictionary<int, UwbHudPlayer> m_Players = new Dictionary<int, UwbHudPlayer>();
        readonly Dictionary<int, UwbPlayerTagItem> m_Tags = new Dictionary<int, UwbPlayerTagItem>();
        readonly List<UwbHudPlayer> m_Red = new List<UwbHudPlayer>(16);
        readonly List<UwbHudPlayer> m_Blue = new List<UwbHudPlayer>(16);
        readonly List<int> m_Gone = new List<int>(8);
        readonly HashSet<int> m_Seen = new HashSet<int>();

        VenuePlayerSync m_Sync;
        Camera m_Camera;
        Canvas m_Canvas;
        bool m_TagOrderDirty;
        float m_OpenTime;

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            m_OpenTime = Time.time;
            EnsureUiCamera();
            if (tagTemplate != null)
                tagTemplate.gameObject.SetActive(false);
            Refresh();
        }

        public override void OnClose()
        {
            ReleaseSync();
            base.OnClose();
        }

        protected override void OnDisable()
        {
            ReleaseSync();
            base.OnDisable();
        }

        // 场景里的 UICamera 默认是关的，Canvas 退回到场景主相机上渲染，会被 3D 地图遮挡。
        // 打开 UICamera，并把 UIRoot 下的各个根 Canvas 指回它。
        static void EnsureUiCamera()
        {
            if (UIRoot.S == null || UIRoot.S.uiCamera == null)
                return;

            Camera uiCamera = UIRoot.S.uiCamera;
            if (!uiCamera.gameObject.activeSelf)
                uiCamera.gameObject.SetActive(true);

            foreach (Canvas canvas in UIRoot.S.GetComponentsInChildren<Canvas>(true))
            {
                if (canvas.isRootCanvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera != uiCamera)
                    canvas.worldCamera = uiCamera;
            }

            // UICamera 带着 "UICamera" 标签时，分割线的 Wave、Lizi 粒子在 UI 相机下几乎看不清，去掉标签后与原版一致。
            uiCamera.gameObject.tag = "Untagged";

            // 分割线等特效是挂在 UI 层的普通粒子，主相机的 CullingMask 也含 UI 层，会从另一个角度再画一遍，叠成发白的虚影。
            if (Camera.main != null)
                Camera.main.cullingMask &= ~uiCamera.cullingMask;
        }

        void ReleaseSync()
        {
            if (m_Sync != null)
                m_Sync.ShowWorldStatus = true;
            m_Sync = null;
        }

        void LateUpdate()
        {
            Refresh();
        }

        void Refresh()
        {
            if (!BindSync())
                return;

            UwbHudData.UseMock = IsMockSource();
            CollectPlayers();
            if (UwbHudData.UseMock)
                UwbHudMock.Apply(m_Players.Values, Time.time - m_OpenTime);

            topBar.Refresh();
            RefreshCards();
            RefreshTags();
        }

        bool BindSync()
        {
            VenuePlayerSync sync = VenuePlayerSync.Current;
            if (sync == null)
                return false;
            if (m_Sync != sync)
            {
                if (m_Sync != null)
                    m_Sync.ShowWorldStatus = true;
                m_Sync = sync;
            }

            if (m_Sync.ShowWorldStatus)
                m_Sync.ShowWorldStatus = false;
            return true;
        }

        static bool IsMockSource()
        {
            UwbRuntime runtime = FindObjectOfType<UwbRuntime>();
            return runtime != null && runtime.ActiveMode == UwbSourceMode.Mock;
        }

        void CollectPlayers()
        {
            m_Views.Clear();
            m_Sync.CollectViews(m_Views);

            m_Seen.Clear();
            for (int i = 0; i < m_Views.Count; i++)
            {
                VenuePlayerView view = m_Views[i];
                if (!m_Players.TryGetValue(view.Id, out UwbHudPlayer player))
                {
                    player = new UwbHudPlayer { number = view.Id };
                    m_Players.Add(view.Id, player);
                    m_TagOrderDirty = true;
                }

                m_Seen.Add(view.Id);
                player.team = view.Team;
                player.name = view.DisplayName;
                player.hp = view.Hp;
                player.maxHp = view.MaxHp;
                player.headWorld = view.HeadWorldPosition;
                player.avatar = UwbHudData.GetAvatar(view.Id);
                if (UwbHudData.TryGetStats(view.Id, out UwbHudData.PlayerStats stats))
                {
                    player.kills = stats.kills;
                    player.deaths = stats.deaths;
                    player.reviveSeconds = stats.reviveSeconds;
                }
            }

            m_Gone.Clear();
            foreach (int id in m_Players.Keys)
            {
                if (!m_Seen.Contains(id))
                    m_Gone.Add(id);
            }

            for (int i = 0; i < m_Gone.Count; i++)
            {
                int id = m_Gone[i];
                m_Players.Remove(id);
                if (m_Tags.TryGetValue(id, out UwbPlayerTagItem tag))
                {
                    Destroy(tag.gameObject);
                    m_Tags.Remove(id);
                }
            }
        }

        void RefreshCards()
        {
            m_Red.Clear();
            m_Blue.Clear();
            foreach (UwbHudPlayer player in m_Players.Values)
                (player.IsRed ? m_Red : m_Blue).Add(player);
            m_Red.Sort(CompareByNumber);
            m_Blue.Sort(CompareByNumber);

            FillCards(redCards, m_Red);
            FillCards(blueCards, m_Blue);
        }

        static void FillCards(UwbPlayerCardItem[] cards, List<UwbHudPlayer> players)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (i < players.Count && i < SlotsPerTeam)
                    cards[i].Apply(players[i]);
                else
                    cards[i].SetEmpty();
            }
        }

        void RefreshTags()
        {
            Camera cam = GetCamera();
            if (cam == null || tagLayer == null)
                return;

            Camera uiCam = GetUiCamera();
            foreach (KeyValuePair<int, UwbHudPlayer> pair in m_Players)
            {
                UwbHudPlayer player = pair.Value;
                if (!m_Tags.TryGetValue(pair.Key, out UwbPlayerTagItem tag))
                {
                    tag = Instantiate(tagTemplate, tagLayer);
                    tag.name = "Tag_" + pair.Key;
                    tag.gameObject.SetActive(true);
                    m_Tags.Add(pair.Key, tag);
                    m_TagOrderDirty = true;
                }

                tag.Apply(player);

                Vector3 screen = cam.WorldToScreenPoint(player.headWorld);
                bool visible = screen.z > 0f;
                if (tag.gameObject.activeSelf != visible)
                    tag.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(tagLayer, screen, uiCam, out Vector2 local))
                    tag.Rect.anchoredPosition = local + tagOffset;
            }

            if (m_TagOrderDirty)
                ReorderTags();
        }

        // 装备序号小的标牌画在上层，整张标牌一起排序，不随距离、血量变化，避免闪烁。
        void ReorderTags()
        {
            m_TagOrderDirty = false;
            List<UwbPlayerTagItem> list = new List<UwbPlayerTagItem>(m_Tags.Values);
            list.Sort((a, b) => b.Number.CompareTo(a.Number));
            for (int i = 0; i < list.Count; i++)
                list[i].transform.SetSiblingIndex(i + 1);
        }

        Camera GetCamera()
        {
            if (m_Camera == null || !m_Camera.isActiveAndEnabled)
                m_Camera = Camera.main;
            return m_Camera;
        }

        Camera GetUiCamera()
        {
            if (m_Canvas == null)
                m_Canvas = GetComponentInParent<Canvas>();
            Canvas root = m_Canvas != null ? m_Canvas.rootCanvas : null;
            if (root == null || root.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return root.worldCamera;
        }

        static int CompareByNumber(UwbHudPlayer a, UwbHudPlayer b)
        {
            return a.number.CompareTo(b.number);
        }
    }
}
