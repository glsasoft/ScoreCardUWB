using UnityEngine;

namespace LetsPiu
{
    public class EditAnimator : MonoBehaviour
    {
        [HideInInspector] public int selectedAnimationIndex;
        [HideInInspector] public float m_CurTime;
        [HideInInspector] public bool isPlaying;
        [HideInInspector] public bool loop;
    }
}