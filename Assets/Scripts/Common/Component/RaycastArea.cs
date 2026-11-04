using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LetsPiu
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer))]
    public class RaycastArea : UnityEngine.UI.MaskableGraphic
    {
        protected override void Awake()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }
    }
}
