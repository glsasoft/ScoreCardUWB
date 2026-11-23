using UnityEngine;

public class TimelineRootMotionAutoRotate : MonoBehaviour
{
    [Header("转向设置")]
    public float turnSpeed = 12f;          // 转向平滑度
    public float minMoveThreshold = 0.001f; // 最小位移阈值，防止静止时抖动
    public bool lockYAxis = true;          // 锁定Y轴（仅水平转向，防止角色上下倾斜）

    private Vector3 lastPosition;

    void OnEnable()
    {
        lastPosition = transform.position;
    }

    // 必须在 LateUpdate 中执行，确保在 Timeline 和 Animator 根运动计算完位移后再转向
    void LateUpdate()
    {
        Vector3 delta = transform.position - lastPosition;

        if (lockYAxis)
        {
            delta.y = 0f;
        }

        // 只有位移超过阈值时才更新旋转，停下时保持当前朝向
        if (delta.sqrMagnitude > minMoveThreshold * minMoveThreshold)
        {
            Quaternion targetRotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }

        // 记录当前帧最终位置供下一帧对比
        lastPosition = transform.position;
    }
}
