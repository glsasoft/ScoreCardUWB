using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LetsPiu
{
    public class SimpleMove : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKey(KeyCode.A))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y,
                    transform.position.z + Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y,
                    transform.position.z - Time.deltaTime);
            }
            
            
        }
    }
}
