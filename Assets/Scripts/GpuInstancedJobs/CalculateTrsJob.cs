using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace.GpuInstancedJobs
{
    [BurstCompile]
    public struct CalculateTrsJob : IJobParallelFor
    {
        private NativeArray<Matrix4x4> _matrices;
        private NativeArray<float3> _positions;
        private readonly float _amplitude;
        private readonly float _frequency;
        private readonly float _time;

        public CalculateTrsJob(float amplitude, float frequency, float time, ref NativeArray<float3> positions, ref NativeArray<Matrix4x4> matrices)
        {
            _amplitude = amplitude;
            _frequency = frequency;
            _time = time;
            _positions = positions;
            _matrices = matrices;
        }

        public void Execute(int index)
        {
            var result = _positions[index].CalculatePositionAndRotationBurst(_amplitude, _frequency, _time);
            _positions[index] = result.pos;
            _matrices[index] = Matrix4x4.TRS(result.pos, result.rot, Vector3.one);
        }
    }
}