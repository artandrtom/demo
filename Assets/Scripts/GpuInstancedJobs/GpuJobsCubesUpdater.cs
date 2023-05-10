using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace.GpuInstancedJobs
{
    public class GpuJobsCubesUpdater : MonoBehaviour
    {
        [SerializeField] private float _yOffset;
        [SerializeField] private int _side;
        [SerializeField] private int _depth;
        [SerializeField] private Material _material;
        [SerializeField] private Mesh _mesh;

        [Range(0.1f, 5f)]
        [SerializeField] private float _amplitude;
        [Range(0.1f, 5f)]
        [SerializeField] private float _frequency;
        
        private RenderParams _rp;
        
        private NativeArray<float3> _positions;
        private NativeArray<Matrix4x4> _matrices;
        
        private CalculateTrsJob _job;
        private JobHandle _jobHandle;

        private void Start()
        {
            var count = _side * _side * _depth;
            _positions = new NativeArray<float3>(count, Allocator.Persistent);
            _matrices = new NativeArray<Matrix4x4>(count, Allocator.Persistent);

            _rp = new RenderParams(_material);
            
            ExtensionMethods.SpawnLoop(_side, _depth, (i, pos) =>
            {
                _positions[i] = pos;
            });
        }
        
        private void Update()
        {
            var time = Time.time;
            _job = new CalculateTrsJob(_amplitude, _frequency, time, ref _positions, ref _matrices);
            _jobHandle = _job.Schedule(_matrices.Length, 64);
        }

        private void LateUpdate()
        {
            _jobHandle.Complete();
            Render();
        }

        private const int Limit = 784;
        
        private void Render()
        {
            for (int i = 0; i < _positions.Length; i++)
            {
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
        
        private void OnDestroy()
        {
            _positions.Dispose();
            _matrices.Dispose();
        }
    }
}