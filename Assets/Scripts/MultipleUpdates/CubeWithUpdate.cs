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
        
    }
}