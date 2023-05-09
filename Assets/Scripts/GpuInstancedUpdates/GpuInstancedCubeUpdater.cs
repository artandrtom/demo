using System;
using UnityEngine;

namespace DefaultNamespace.GpuInstancedUpdates
{
    public class GpuInstancedCubeUpdater : MonoBehaviour
    {
        [SerializeField] private int _side;
        [SerializeField] private int _depth;
        [SerializeField] private Material _material;
        [SerializeField] private Mesh _mesh;
        
        private Matrix4x4[] _matrices;
        private Vector3[] _positions;
        private RenderParams _rp;

        private void Start()
        {
            var count = _side * _side * _depth;
            _positions = new Vector3[count];
            _matrices = new Matrix4x4[count];

            ExtensionMethods.SpawnLoop(_side, _depth, (i, pos) =>
            {
                _positions[i] = pos;
            });
            
            _rp = new RenderParams(_material);
        }

        private const int Limit = 784;
        
        private void Update()
        {
            var time = Time.time;
            for (int i = 0; i < _positions.Length; i++)
            {
                var result = _positions[i].CalculatePositionAndRotation(time);
                _matrices[i].SetTRS(result.pos, result.rot, Vector3.one);
                _positions[i].y = result.pos.y;
                if (i >= Limit && i % Limit == 0)
                {
                    Graphics.RenderMeshInstanced(_rp, _mesh, 0, _matrices, instanceCount:Limit, startInstance:i - Limit);
                }
            }

            if (_positions.Length > Limit)
            {
                var mod = _positions.Length % Limit;
                if (mod > 0)
                {
                    Graphics.RenderMeshInstanced(_rp, _mesh, 0, _matrices, mod, startInstance:_positions.Length - mod - 1);
                }
            }
        }
    }
}