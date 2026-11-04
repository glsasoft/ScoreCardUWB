using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirza.Solaris
{
    public class SmoothRotationController : MonoBehaviour
    {
        public float speed = 90.0f;
        public float smoothTime = 0.1f;

        float targetRotationY;

        Vector3 currentVelocity;

        void Start()
        {

        }

        void OnEnable()
        {
            targetRotationY = transform.rotation.eulerAngles.y;
        }

        void Update()
        {
            float input = Input.GetAxis("Horizontal");

            targetRotationY += (input * speed) * Time.deltaTime;

            Quaternion targetRotation = Quaternion.Euler(0.0f, targetRotationY, 0.0f);

            //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTime);
            //transform.rotation = SmoothDamp(transform.rotation, targetRotation, ref deriv, smoothTime);

            Vector3 eulerAngles;

            eulerAngles.x = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.x, ref currentVelocity.x, smoothTime);
            eulerAngles.y = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.y, targetRotation.eulerAngles.y, ref currentVelocity.y, smoothTime);
            eulerAngles.z = Mathf.SmoothDampAngle(transform.rotation.eulerAngles.z, targetRotation.eulerAngles.z, ref currentVelocity.z, smoothTime);

            transform.rotation = Quaternion.Euler(eulerAngles);
        }
    }
}
