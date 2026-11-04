using UnityEngine;

namespace LetsPiu.Base.BaseScripts.Define.ModelDefine.Event
{
    public enum KeyEvent
    {
        KeyUp,
        KeyDown,
        KeyPress,
        KeyLongPress,
    }
    public class GameKeyEvent
    {
        public KeyCode keyCode;
        public KeyEvent keyEvent;
    }
}