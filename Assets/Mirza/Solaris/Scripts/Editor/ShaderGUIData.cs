
using UnityEngine;
using UnityEngine.Serialization;

namespace Mirza.Solaris
{
    [CreateAssetMenu(fileName = "ShaderGUI Data", menuName = "Mirza/Shader/ShaderGUI Data")]
    public class ShaderGUIData : ScriptableObject
    {
        [System.Serializable]
        public class NameText
        {
            [FormerlySerializedAs("name")]
            public string name;

            [FormerlySerializedAs("text")]
            [TextArea(2, 8)]
            public string text;
        }

        // ...

        public string title;

        [TextArea(2, 8)]
        public string description;

        // ...

        [FormerlySerializedAs("data")]
        public NameText[] tooltips;
    }
}