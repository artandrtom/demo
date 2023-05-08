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
            var finish = Time.time + duration;
            while (Time.time < finish)
            {
                transform.Translate(Vector3.right * (Time.deltaTime * xOffset) / duration);
                await Task.Yield();
            }
        }
        
        //struct Delay()
    }
}