using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.MultipleUpdates
{
    public sealed class CubesUpdater : MonoBehaviour
    {
        private readonly List<CubeWithUpdate> _cubes = new List<CubeWithUpdate>();

        public static CubesUpdater Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void Register(CubeWithUpdate cubeWithUpdate)
        {
            _cubes.Add(cubeWithUpdate);
        }
        
        private void Update()
        {

        }
        
    }
}