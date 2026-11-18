using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class TrailMaxLength : MonoBehaviour
{
    public float maxTrailLength = 5f;
    private TrailRenderer _trail;
    private Vector3 _lastPos;

    void Start()
    {
        _trail = GetComponent<TrailRenderer>();
        _lastPos = transform.position;
    }

    void Update()
    {
        float moveDelta = (transform.position - _lastPos).magnitude;
        float speed = moveDelta / Time.deltaTime;
        _lastPos = transform.position;

        if (speed > 0.001f)
        {
            // 动态设置存活时间，保证拖尾不会超过maxTrailLength
            _trail.time = maxTrailLength / speed;
        }
        else
        {
            // 几乎不动时，给一个很小的基础时间，避免拖尾残留过长
            _trail.time = 0.05f;
        }
    }
}
