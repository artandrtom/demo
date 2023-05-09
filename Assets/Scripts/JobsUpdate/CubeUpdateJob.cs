using UnityEngine.Jobs;

namespace DefaultNamespace.JobsUpdate
{
    public readonly struct CubeUpdateJob : IJobParallelForTransform
    {
        private readonly float _time;

        public CubeUpdateJob(float time)
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