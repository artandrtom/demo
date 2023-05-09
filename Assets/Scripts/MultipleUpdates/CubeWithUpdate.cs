using System;
using UnityEngine;

namespace DefaultNamespace.MultipleUpdates
{
    public sealed class CubeWithUpdate : MonoBehaviour
    {
        private void Update()
        {
            transform.UpdatePositionAndRotation(Time.time);
        }
        
        public void Rotate()
        {
            transform.Rotate(Vector3.up, Time.deltaTime * 150, Space.Self);
        }
        

    }
}