using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirza.Solaris
{
    //[ExecuteInEditMode]
    public class TransformDeltaToShader : MonoBehaviour
    {
        public Material material;

        [Space]

        public float movementDeltaScaleXZ = 1.0f;
        public float movementDeltaScaleY = 1.0f;

        [Space]

        public float rotationDeltaScale = 1.0f;

        [Space]

        //public float smoothMovementDeltaSpeed = 8.0f;

        //public float smoothMovementDeltaSmoothDampTime = 0.1f;
        //public float smoothRotationDeltaSmoothDampTime = 0.1f;

        [Space]

        public float noiseOffsetFromMovementScale = 1.0f;
        public float vertexNoiseOffsetFromMovementScale = 1.0f;

        Vector3 previousPosition;
        Quaternion previousRotation;

        Vector3 movementDelta;
        //Vector3 smoothMovementDelta;

        Vector3 rotationDelta;
        //Vector3 smoothRotationDelta;

        //Vector3 smoothMovementDeltaVelocity;
        //Vector3 smoothRotationDeltaVelocity;

        Vector3 noiseOffset;
        Vector3 vertexNoiseOffset;

        void OnEnable()
        {
            previousPosition = transform.position;
            previousRotation = transform.rotation;

            noiseOffset = Vector3.zero;
            vertexNoiseOffset = Vector3.zero;
        }

        void ProcessUpdate()
        {
            Vector3 currentPosition = transform.position;
            movementDelta = (currentPosition - previousPosition) / Time.deltaTime;

            Vector3 movementDeltaScale = new(movementDeltaScaleXZ, movementDeltaScaleY, movementDeltaScaleXZ);

            // Scaling is done in local space.
            // This way, if I rotate the object, the scaling will be correct along the LOCAL axes (example, if I want less scaling along the length).

            Vector3 localMovementDelta = transform.InverseTransformDirection(movementDelta);
            localMovementDelta.Scale(movementDeltaScale);

            movementDelta = transform.TransformDirection(localMovementDelta);

            // Rotation. UPDATE: Issues with Euler angles wrapping, using Quaternions instead.

            Quaternion currentRotation = transform.rotation;
            Quaternion deltaRotation = currentRotation * Quaternion.Inverse(previousRotation);

            deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);

            rotationDelta = axis * (angle / Time.deltaTime);

            previousPosition = currentPosition;
            previousRotation = transform.rotation;
        }

        void LateUpdate()
        {
            ProcessUpdate();

            // Position.

            //smoothMovementDelta = Vector3.SmoothDamp(smoothMovementDelta, movementDelta, ref smoothMovementDeltaVelocity, smoothMovementDeltaSmoothDampTime);

            material.SetVector("_VertexOffsetOverY1", -movementDelta);

            // Rotation.

            Vector3 rotationDeltaSwapped = new(rotationDelta.y, 0.0f, rotationDelta.x);
            material.SetVector("_VertexOffsetOverCircularY", rotationDeltaSwapped * rotationDeltaScale);

            float smoothMovementDeltaMagnitude = movementDelta.magnitude * Time.deltaTime;

            // Noise offset.

            noiseOffset.y += smoothMovementDeltaMagnitude * noiseOffsetFromMovementScale;
            vertexNoiseOffset.y += smoothMovementDeltaMagnitude * vertexNoiseOffsetFromMovementScale;

            material.SetVector("_NoiseOffset", noiseOffset);
            material.SetVector("_VertexNoiseOffset", vertexNoiseOffset);
        }
    }
}