using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirza.Solaris
{
    public class SetFPSOnStart : MonoBehaviour
    {
        public int targetFPS = 60;

        void Start()
        {
            Application.targetFrameRate = targetFPS;
        }
    }
}