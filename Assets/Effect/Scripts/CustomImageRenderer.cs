using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class CustomImageRenderer : UIBehaviour
{
    [SerializeField] List<Transform> m_PointList;
    [SerializeField] Color m_color;
    private CanvasRenderer canvasRenderer;
    Mesh m_Mesh;

    protected override void Awake()
    {
        base.Awake();
        canvasRenderer = GetComponent<CanvasRenderer>();
        m_Mesh = new Mesh();
    }

    protected override void Start()
    {
        base.Start();
        var uvCount = (m_PointList.Count - 2) * 3;
        m_Mesh.vertices = new Vector3[uvCount];
        m_Mesh.normals = new Vector3[uvCount];
        m_Mesh.tangents = new Vector4[uvCount];
        m_Mesh.colors = new Color[uvCount];
        for (int i = 0; i < m_PointList.Count - 2; i++)
        {
            m_Mesh.vertices[i * 3] = m_PointList[0].localPosition;
            m_Mesh.vertices[i * 3 + 1] = m_PointList[i + 1].localPosition;
            m_Mesh.vertices[i * 3 + 2] = m_PointList[i + 2].localPosition;
            //vertices.Add(GetNewUV(m_PointList[0].localPosition));
            //vertices.Add(GetNewUV(m_PointList[i + 1].localPosition));
            //vertices.Add(GetNewUV(m_PointList[i + 2].localPosition));
        }

        for (int i = 0; i < m_Mesh.normals.Length; i++)
        {
            m_Mesh.normals[i] = new Vector3(0, 0, -1);
        }

        for (int i = 0; i < m_Mesh.tangents.Length; i++)
        {
            m_Mesh.tangents[i] = new Vector4(0, 0, 0, -1);
        }

        for (int i = 0; i < m_Mesh.colors.Length; i++)
        {
            m_Mesh.colors[i] = m_color;
        }
        canvasRenderer.SetMesh(m_Mesh);

        //canvasRenderer.SetVertices(vertices, 4);
    }

    UIVertex GetNewUV(Vector2 pos)
    {
        UIVertex vertex = new()
        {
            position = pos,
            normal = new Vector3(0, 0, -1),
            tangent = new Vector3(0, 0, -1),
            color = m_color
        };
        return vertex;
    }
}
