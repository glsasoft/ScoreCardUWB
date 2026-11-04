namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Http.App
{
    public class GameModeInfo:BaseEntity
    {
        /// <summary>
        /// 游戏模式name
        /// </summary>
        public string name;
        public string nameEn;


        /// <summary>
        /// 描述
        /// </summary>
        public string detail;

        ///<summary>
        ///模式规则
        /// </summary>
        public string rule;

        public string type;
        public string avatarName;
        public int lineType;// 1.线下 2.线上 
    }
}