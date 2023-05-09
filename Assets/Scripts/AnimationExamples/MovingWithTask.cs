using System;
using System.Threading.Tasks;
using UnityEngine;

namespace DefaultNamespace.AnimationExamples
{
    public class MovingWithTask : MonoBehaviour
    {
        public async Task Move(float delay, float duration, float xOffset)
        {
            await Task.Delay(TimeSpan.FromSeconds(delay));
            float time = 0f;
            float progress = 0f;
            var initial = transform.position;
            var targetPos = new Vector3(initial.x + xOffset, initial.y, initial.z);
            while (progress < 1f)
            {
                progress = time / duration;
                transform.position = Vector3.Lerp(initial, targetPos, progress);
                await Task.Yield();
                time += Time.deltaTime;
            }
        }
        
        //struct Delay()
    }
}