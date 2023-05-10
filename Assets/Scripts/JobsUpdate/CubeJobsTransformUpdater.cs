using System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace DefaultNamespace.JobsUpdate
{
    public class CubeJobsTransformUpdater : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private float _yOffset;
        [SerializeField] private int _side;
        [SerializeField] private int _depth;
        
        private Transform[] _transforms;
        
        [ReadOnly] 
        private TransformAccessArray _transformAccessArray;

        private UpdateCubeTransformsJob _job;
        
        private JobHandle _jobHandle;

        private void Start()
        {
            _transforms = new Transform[_side * _side * _depth];
            ExtensionMethods.SpawnLoop(_side, _depth, (i, pos) =>
            {
                var go = Instantiate(_prefab, new Vector3(pos.x, pos.y * _yOffset, pos.z), Quaternion.identity, transform);
                _transforms[i] = go.transform;
            });
            _transformAccessArray = new TransformAccessArray(_transforms);
        }
        
        private void Update()
        {
            var time = Time.time;
            _job = new UpdateCubeTransformsJob(time);
            _jobHandle = _job.Schedule(_transformAccessArray);
        }

        private void LateUpdate()
        {
            _jobHandle.Complete();
        }
        
        private void OnDestroy()
        {
            _transformAccessArray.Dispose();
        }
    }
}