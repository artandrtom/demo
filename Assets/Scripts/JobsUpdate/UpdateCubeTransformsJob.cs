using Unity.Burst;
using UnityEngine.Jobs;

namespace DefaultNamespace.JobsUpdate
{
    //[BurstCompile]
    public readonly struct UpdateCubeTransformsJob : IJobParallelForTransform
    {
        private readonly float _time;

        public UpdateCubeTransformsJob(float time)
        {
            _time = time;
        }
        
        public void Execute(int index, TransformAccess transform)
        {
            var result = transform.position.CalculatePositionAndRotation(_time);
            transform.position = result.pos;
            transform.rotation = result.rot;
        }
    }
}