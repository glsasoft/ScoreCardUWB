using UnityEngine;

namespace LetsPiu
{
    public abstract class ProgressBarEffectBase : MonoBehaviour
    {
        [Range(0, 1)] public float progress;
    }
}
