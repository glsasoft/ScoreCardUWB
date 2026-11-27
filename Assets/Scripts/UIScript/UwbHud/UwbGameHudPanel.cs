using System.Collections.Generic;
using LetsPiu.Character;
using LetsPiu.Character.Uwb;
using UnityEngine;

namespace LetsPiu.UI
{
    /// <summary>
    /// UWB 单屏比分牌「游戏中」HUD：顶栏、底部队员卡（10/20/40 档）、头顶标牌。
    /// 底栏按开局参赛人数锁档（策划 P10–12），本局不随存活人数换档。
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class UwbGameHudPanel : AbstractPanel
    {
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
        readonly List<UwbHudPlayer> m_Red = new List<UwbHudPlayer>(32);
        readonly List<UwbHudPlayer> m_Blue = new List<UwbHudPlayer>(32);
        readonly List<int> m_Gone = new List<int>(8);
        readonly HashSet<int> m_Seen = new HashSet<int>();

        VenuePlayerSync m_Sync;
        Camera m_Camera;
        Canvas m_Canvas;
        bool m_TagOrderDirty;
        float m_OpenTime;

        UwbHudRosterMode m_Mode = UwbHudRosterMode.Ten;
        bool m_ModeLocked;
        const float CardGap = 3f;

        int m_PeakRoster;
        int m_SeenRosterEpoch = -1;
        bool m_BaselineReady;
        Vector2 m_RedOrigin;
        Vector2 m_BlueOrigin;
        float m_BaseCardW;
        float m_BaseCardH;
        float m_BaseStep;
        float m_RedStepSign = 1f;
        float m_BlueStepSign = 1f;
        Vector3 m_BaseScale = Vector3.one;

        public override void OnOpen(object[] args)
        {
            base.OnOpen(args);
            m_OpenTime = Time.time;
            m_ModeLocked = false;
            m_PeakRoster = 0;
            m_SeenRosterEpoch = UwbHudData.RosterLayoutEpoch;
            EnsureUiCamera();
            CaptureBaselineIfNeeded();
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

        public static void EnsureUiCamera()
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

            uiCamera.gameObject.tag = "Untagged";

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

            bool hasMatch = ScoreCardRuntime.ScoreCardMatchDataHub.Current != null
                            || ScoreCardRuntime.ScoreCardModeMockHost.IsRunning;
            UwbHudData.UseMock = !hasMatch && IsMockSource();
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

            UpdateRosterMode(m_Players.Count);
            int slots = UwbHudRosterModeUtil.SlotsPerTeam(m_Mode);
            FillCards(redCards, m_Red, slots);
            FillCards(blueCards, m_Blue, slots);
        }

        void UpdateRosterMode(int rosterCount)
        {
            if (UwbHudData.RosterLayoutEpoch != m_SeenRosterEpoch)
            {
                m_SeenRosterEpoch = UwbHudData.RosterLayoutEpoch;
                m_ModeLocked = false;
                m_PeakRoster = 0;
                m_OpenTime = Time.time;

                // 已知新一场人数时立刻按它选档并锁定，不用等玩家重建出来
                int expected = UwbHudData.ExpectedRoster;
                if (expected > 0)
                {
                    m_PeakRoster = expected;
                    m_ModeLocked = true;
                    UwbHudRosterMode expectedMode = UwbHudRosterModeUtil.FromCount(expected);
                    if (expectedMode != m_Mode || !m_BaselineReady)
                    {
                        m_Mode = expectedMode;
                        ApplyRosterLayout(expectedMode);
                    }

                    return;
                }
            }

            if (rosterCount > m_PeakRoster)
                m_PeakRoster = rosterCount;

            if (m_ModeLocked)
                return;

            // 人数还在涨时按峰值选档；达到该档容量或开局数秒后锁定，避免中途跳档。
            UwbHudRosterMode mode = UwbHudRosterModeUtil.FromCount(Mathf.Max(rosterCount, m_PeakRoster));
            if (mode != m_Mode || !m_BaselineReady)
            {
                m_Mode = mode;
                ApplyRosterLayout(mode);
            }

            int need = UwbHudRosterModeUtil.SlotsPerTeam(mode) * 2;
            if (m_PeakRoster >= need || (rosterCount > 0 && Time.time - m_OpenTime > 3f && m_PeakRoster >= 8))
                m_ModeLocked = true;
        }

        void CaptureBaselineIfNeeded()
        {
            if (m_BaselineReady)
                return;
            if (redCards == null || redCards.Length == 0 || redCards[0] == null)
                return;

            RectTransform r0 = redCards[0].Rect;
            m_RedOrigin = r0.anchoredPosition;
            m_BaseCardW = r0.sizeDelta.x;
            m_BaseCardH = r0.sizeDelta.y;
            m_BaseScale = r0.localScale;
            m_BaseStep = m_BaseCardW + 6f;
            m_RedStepSign = 1f;
            if (redCards.Length > 1 && redCards[1] != null)
            {
                float dx = redCards[1].Rect.anchoredPosition.x - r0.anchoredPosition.x;
                if (Mathf.Abs(dx) > 1f)
                {
                    m_BaseStep = Mathf.Abs(dx);
                    m_RedStepSign = Mathf.Sign(dx);
                }
            }

            m_BlueStepSign = -1f;
            if (blueCards != null && blueCards.Length > 0 && blueCards[0] != null)
            {
                m_BlueOrigin = blueCards[0].Rect.anchoredPosition;
                if (blueCards.Length > 1 && blueCards[1] != null)
                {
                    float bdx = blueCards[1].Rect.anchoredPosition.x - m_BlueOrigin.x;
                    if (Mathf.Abs(bdx) > 1f)
                        m_BlueStepSign = Mathf.Sign(bdx);
                }
            }
            else
                m_BlueOrigin = m_RedOrigin;

            m_BaselineReady = true;
        }

        void ApplyRosterLayout(UwbHudRosterMode mode)
        {
            CaptureBaselineIfNeeded();
            if (!m_BaselineReady)
                return;

            int slots = UwbHudRosterModeUtil.SlotsPerTeam(mode);
            EnsureTeamCards(ref redCards, slots, true);
            EnsureTeamCards(ref blueCards, slots, false);

            int cols;
            int rows;
            float cardW;
            float cardH;
            float step;
            float rowStep = 0f;
            float visualW = m_BaseCardW * m_BaseScale.x;
            float visualH = m_BaseCardH * m_BaseScale.y;
            if (mode == UwbHudRosterMode.Ten)
            {
                cols = 5;
                rows = 1;
                cardW = m_BaseCardW;
                cardH = m_BaseCardH;
                step = m_BaseStep;
            }
            else
            {
                // 在 10 人档整行宽度内排满 10 列：9 个列间距 + 一张卡宽 = 整行宽度
                cols = 10;
                rows = mode == UwbHudRosterMode.Forty ? 2 : 1;
                float totalW = m_BaseStep * 4f + visualW;
                step = (totalW + CardGap) / cols;
                cardW = (step - CardGap) / m_BaseScale.x;
                cardH = rows == 1 ? m_BaseCardH : (m_BaseCardH - CardGap / m_BaseScale.y) * 0.5f;
                rowStep = cardH * m_BaseScale.y + CardGap;
            }

            LayoutTeam(redCards, slots, cols, rows, m_RedOrigin, step * m_RedStepSign, cardW, cardH, rowStep, mode);
            LayoutTeam(blueCards, slots, cols, rows, m_BlueOrigin, step * m_BlueStepSign, cardW, cardH, rowStep, mode);

            foreach (UwbPlayerTagItem tag in m_Tags.Values)
                tag.ApplyRosterDensity(mode);
        }

        void EnsureTeamCards(ref UwbPlayerCardItem[] cards, int needed, bool isRed)
        {
            if (cards == null || cards.Length == 0 || cards[0] == null)
                return;

            var list = new List<UwbPlayerCardItem>(Mathf.Max(cards.Length, needed));
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] != null)
                    list.Add(cards[i]);
            }

            UwbPlayerCardItem template = list[0];
            Transform parent = template.transform.parent;
            while (list.Count < needed)
            {
                UwbPlayerCardItem clone = Instantiate(template, parent);
                clone.name = (isRed ? "RedCard_" : "BlueCard_") + (list.Count + 1);
                clone.gameObject.SetActive(true);
                list.Add(clone);
            }

            for (int i = 0; i < list.Count; i++)
                list[i].gameObject.SetActive(i < needed);

            cards = list.ToArray();
        }

        void LayoutTeam(
            UwbPlayerCardItem[] cards,
            int slots,
            int cols,
            int rows,
            Vector2 origin,
            float step,
            float cardW,
            float cardH,
            float rowStep,
            UwbHudRosterMode mode)
        {
            bool keepEmpty = mode == UwbHudRosterMode.Ten;
            for (int i = 0; i < cards.Length; i++)
            {
                UwbPlayerCardItem card = cards[i];
                if (card == null)
                    continue;

                if (i >= slots)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                int row = rows <= 1 ? 0 : i / cols;
                int col = rows <= 1 ? i : i % cols;
                RectTransform rt = card.Rect;
                rt.localScale = m_BaseScale;
                rt.sizeDelta = new Vector2(cardW, cardH);
                // 锚点按 BottomCenter：y 向上增大，第一行在最上面，origin 是最下面一行
                rt.anchoredPosition = new Vector2(
                    origin.x + col * step,
                    origin.y + (rows - 1 - row) * rowStep);
                card.ApplyRosterDensity(mode, keepEmpty, cardW, cardH);
            }
        }

        static void FillCards(UwbPlayerCardItem[] cards, List<UwbHudPlayer> players, int slots)
        {
            if (cards == null)
                return;
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null)
                    continue;
                if (i < slots && i < players.Count)
                    cards[i].Apply(players[i]);
                else if (i < slots)
                    cards[i].SetEmpty();
                else
                    cards[i].gameObject.SetActive(false);
            }
        }

        void RefreshTags()
        {
            Camera cam = GetCamera();
            if (cam == null || tagLayer == null || tagTemplate == null)
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
                    tag.ApplyRosterDensity(m_Mode);
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
