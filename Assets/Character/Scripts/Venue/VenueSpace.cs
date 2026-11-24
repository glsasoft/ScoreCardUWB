using UnityEngine;

namespace LetsPiu.Character
{
    /// <summary>
    /// 场馆坐标到 Unity 场景的对应关系。
    /// 把这个物体放在模拟场景里真实原点的位置，朝向对准场馆 +Z。
    /// 场景按 1:1 搭的话，unityUnitsPerMeter 保持 1。
    /// </summary>
    [DisallowMultipleComponent]
    public class VenueSpace : MonoBehaviour
    {
        [Tooltip("场馆 (0,0,0) 在场景里的锚点。留空就用自己。")]
        public Transform origin;

        [Tooltip("场馆里的 1 米等于场景里多少单位。")]
        public float unityUnitsPerMeter = 1f;

        Transform Anchor => origin != null ? origin : transform;

        public Vector3 ToWorld(Vector3 venueMeters)
        {
            return Anchor.TransformPoint(venueMeters * unityUnitsPerMeter);
        }

        public Vector3 FromWorld(Vector3 world)
        {
            float scale = Mathf.Max(0.0001f, unityUnitsPerMeter);
            return Anchor.InverseTransformPoint(world) / scale;
        }

        //void OnDrawGizmos()
        //{
        //    var anchor = Anchor;
        //    float length = 2f * Mathf.Max(0.01f, unityUnitsPerMeter);
        //    Vector3 p = anchor.position;
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawLine(p, p + anchor.right * length);
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawLine(p, p + anchor.up * length);
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawLine(p, p + anchor.forward * length);
        //}
    }
}
