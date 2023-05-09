﻿using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using UnityEngine;

namespace DefaultNamespace
{
    public static class ExtensionMethods
    {
        public static float WaitWhenAll(this IEnumerable<CoroutineHandle> handles)
        {
            return Timing.WaitUntilTrue(() =>
            {
                return handles.All(e => !e.IsRunning);
            });
        }
        
        public static float WaitForCompletion(this CoroutineHandle handle)
        {
            return Timing.WaitUntilTrue(() => !handle.IsRunning);
        }
        
        public static void SpawnLoop(int side, int depth, Action<int, Vector3> callback)
        {
            var i = 0;
            for (int y = 0; y < depth; y++)
            {
                for (int x = 0; x < side; x++)
                {
                    for (int z = 0; z < side; z++)
                    {
                        callback(i++, new Vector3(x, y, z));
                    }
                }
            }
        }
        
        private const float Amplitude = 2f;
        private const float Frequency = 0.5f;
        private static readonly Quaternion TargetRot = Quaternion.Euler(45f, 45f, 45f);

        public static void UpdatePositionAndRotation(this Transform transform, float time)
        {
            var pos = transform.position;
            var result = pos.CalculatePositionAndRotation(time);
            transform.SetPositionAndRotation(result.pos, result.rot);
        }

        public static (Vector3 pos, Quaternion rot) CalculatePositionAndRotation(this Vector3 pos, float time)
        {
            var factor = Mathf.Sin(Frequency * pos.x + time) + Mathf.Cos(Frequency * pos.z + time);
            float y = 0 + Amplitude * factor;
            var rot = Quaternion.Slerp(Quaternion.identity, TargetRot, factor);
            return (new Vector3(pos.x, y, pos.z), rot);
        }
    }
}