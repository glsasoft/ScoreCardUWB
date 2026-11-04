// 飘字提示管理器：线程安全、生命周期安全、异步无阻塞
// 调用方式：ToastPanel.Show("任意内容");
using Cysharp.Threading.Tasks;
using LetsPiu;
using LetsPiu.UIDefine;
using System.Threading;
using UnityEngine;

public class ToastPanel : MonoBehaviour
{
    private const int TopSortingOrder = short.MaxValue; //sortingOrder 取值范围是 -32768 ~ 32767，即短整型范围。

    [Header("动画参数")]
    [SerializeField] private float fadeInDuration  = 0.3f;
    [SerializeField] private float lifeDuration    = 1f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private static ToastPanel _instance;
    private static CancellationTokenSource _cts = new();

    public static ToastPanel Instance
    {
        get
        {
#if UNITY_EDITOR
            if (_instance == null)
                _instance = FindObjectOfType<ToastPanel>(includeInactive: true);
#endif
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        EnsureTopmostCanvas();
    }

    /// <summary>
    /// 保持 Toast 为独立顶层根 Canvas，改用 ScreenSpaceOverlay + 高 sortingOrder，
    /// 确保盖在同为 Overlay 的面板（如 UIDev）之上。不要 reparent，否则会丢失根 Canvas
    /// 的自动缩放（被父级 scale 链接管而变成 0，导致看不见）。
    /// </summary>
    private void EnsureTopmostCanvas()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) return;

        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = TopSortingOrder;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
    }

    public static void Show(string msg)
    {
        UniTask.Void(async () =>
        {
            await UniTask.SwitchToMainThread();

            // 极端兜底：万一场景里确实没有 ToastPanel
            if (_instance == null)
            {
#if UNITY_EDITOR
                _instance = FindObjectOfType<ToastPanel>(includeInactive: true);
#endif
                if (_instance == null)
                {
                    Debug.LogError("[ToastPanel] 场景中没有 ToastPanel，无法显示飘字！");
                    return;
                }
            }
            await _instance.SpawnToastAsync(msg, _cts.Token);
        });
    }

    /// <summary> 内部异步生成飘字 </summary>
    private async UniTask SpawnToastAsync(string msg, CancellationToken token)
    {
        await UniTask.NextFrame(token);     // 确保对象池/UI 就绪

        GameObject toastGO = ToastItemPool.Instance.GetItem();
        if (toastGO == null)
        {
            Debug.LogError("[ToastPanel] 对象池没有可用 ToastItem！");
            return;
        }
        toastGO.GetComponent<ToastItem>()
               .Init(msg, fadeInDuration, lifeDuration, fadeOutDuration);
    }
}