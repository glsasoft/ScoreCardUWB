using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LetsPiu.Base
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public class CustomQuadMesh : MonoBehaviour
    {
        public Vector2 size = Vector2.one;
        public Vector2 minUV = Vector2.zero;
        public Vector2 maxUV = Vector2.one;

        private Vector2 lastSize = Vector2.one;
        private Vector2 lastMinUV = Vector2.zero;
        private Vector2 lastMaxUV = Vector2.one;

        private MeshFilter meshFilter;
        private Mesh mesh;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.name = "CustomQuadMesh";
            mesh.vertices = new Vector3[]
            {
                new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0), new Vector3(-size.x * 0.5f, size.y * 0.5f, 0),
                new Vector3(size.x * 0.5f, size.y * 0.5f, 0), new Vector3(size.x * 0.5f, -size.y * 0.5f, 0)
            };
            mesh.uv = new Vector2[]
            {
                minUV, new Vector2(minUV.x, maxUV.y), maxUV, new Vector2(maxUV.x, minUV.y)
            };
            mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            meshFilter.sharedMesh = mesh;

            lastSize = size;
            lastMinUV = minUV;
            lastMaxUV = maxUV;
        }

        private void OnDestroy()
        {
            //Object.Destroy(mesh);
            Object.DestroyImmediate(mesh);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mesh == null)
                return;

            if (lastSize != size)
            {
                mesh.vertices = new Vector3[]
                {
                new Vector3(-size.x * 0.5f, -size.y * 0.5f, 0), new Vector3(-size.x * 0.5f, size.y * 0.5f, 0),
                new Vector3(size.x * 0.5f, size.y * 0.5f, 0), new Vector3(size.x * 0.5f, -size.y * 0.5f, 0)
                };
                mesh.RecalculateNormals();

                lastSize = size;
            }

            minUV.x = Mathf.Clamp01(minUV.x);
            minUV.y = Mathf.Clamp01(minUV.y);
            maxUV.x = Mathf.Clamp01(maxUV.x);
            maxUV.y = Mathf.Clamp01(maxUV.y);

            if (lastMinUV != minUV || lastMaxUV != maxUV)
            {
                mesh.uv = new Vector2[]
                {
                    minUV, new Vector2(minUV.x, maxUV.y), maxUV, new Vector2(maxUV.x, minUV.y)
                };

                lastMinUV = minUV;
                lastMaxUV = maxUV;
            }
        }

        [ContextMenu("SaveAs")]
        public void SaveAs()
        {
            if (mesh == null)
                return;

            AssetDatabase.CreateAsset(mesh, "Assets/CustomQuadMesh.mesh");
        }
#endif
    }
}
