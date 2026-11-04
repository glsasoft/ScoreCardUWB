namespace LetsPiu.Base
{
    public partial class TDAudioResource
    {
        public enum AudioKind
        {
            None = 0,
            BackgroundMusic = 1,
            GameSound = 2,
            CharacterSound = 3,
            KeySound = 4,
        }

        /// <summary>
        /// 完成数据解析 方法名和参数不可修改
        /// </summary>
        public void Reset()
        {
            
        }

        public AudioKind audioKind => (AudioKind)Kind;
    }
}