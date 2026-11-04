using UnityEngine;

namespace LetsPiu.Component
{
    public class ExtSingleScrollView : SingleScrollView
    {
        public int GetFirstItemPosition()
        {
            return OnGetPosIndex(new Vector2(20, -30));
        }
    }
}