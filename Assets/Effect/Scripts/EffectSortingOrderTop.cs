using UnityEngine;

namespace LetsPiu.Base
{
    /// <summary>
    /// 启用时把自身及子节点所有 Renderer 的 SortingOrder 提到最高，用于运行时把特效拖进场景测试。
    /// </summary>
    public class EffectSortingOrderTop : MonoBehaviour
    {
        private void OnEnable()
        {
            foreach (var r in GetComponentsInChildren<Renderer>(true))
                r.sortingOrder = short.MaxValue;
        }
    }
}
