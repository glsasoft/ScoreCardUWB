using JetBrains.Annotations;
using LetsPiu.Define;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.Accessories
{
    /// <summary>
    /// 根据配件id获取配件列表参数
    /// </summary>
    public class GetGunAccessoriesListParams
    {
        /// <summary>
        /// 游戏模式ID
        /// </summary>
        public GameMode gameModeId;
        /// <summary>
        /// 不传就是全部
        /// </summary>
        [CanBeNull] public string accessoriesId;
    }
}