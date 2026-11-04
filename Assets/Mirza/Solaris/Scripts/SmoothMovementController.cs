using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirza.Solaris
{
    public class SmoothMovementController : MonoBehaviour
    {
        public float speed = 1.0f;
        public float smoothTime = 0.1f;

        Vector3 targetPosition;
        Vector3 currentVelocity;

        void Start()
        {

        }

        void OnEnable()
        {
            targetPosition = transform.position;
        }

        void Update()
        {
            Vector2 input = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            Vector3 movement = new Vector3(input.x, 0.0f, input.y).normalized;

            targetPosition += (movement * speed) * Time.deltaTime;

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

            Debug.DrawLine(transform.position, targetPosition, Color.red);
        }
    }
}