namespace LetsPiu.Base
{
    public static partial class TDEffectResourceTable
    {
        static readonly bool m_IsReadAllData = false;

        public static float GetExistTime(int effectID)
        {
            if (effectID <= 0)
            {
                return 0f;
            }

            TDEffectResource td = GetData(effectID);
            if (td != null)
            {
                return (td.DelayDestroyTime + td.Duration) / 1000;
            }

            return 0f;
        }

        /// <summary>
        /// 清理表格 方法名和参数不可修改
        /// </summary>
        static void ClearExtend()
        {

        }

        /// <summary>
        /// 完成配置解析 方法名和参数不可修改
        /// </summary>
        static void CompleteExtend()
        {

        }

        /// <summary>
        /// 完成某行解析 方法名和参数不可修改
        /// </summary>
        static void CompleteRowHead(int key)
        {

        }

        /// <summary>
        /// 添加某行 方法名和参数不可修改
        /// </summary>
        static void CompleteRowAdd(TDEffectResource tdData)
        {

        }
    }
}