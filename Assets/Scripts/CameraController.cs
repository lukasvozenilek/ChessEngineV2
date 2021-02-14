using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class CameraController : MonoBehaviour
    {
        private void Update()
        {
            transform.rotation = Quaternion.Euler(0,0,GameState.BlackPerspective? 180 : 0);
        }
    }
}