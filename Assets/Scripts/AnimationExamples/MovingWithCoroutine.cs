using System;
using System.Collections;
using UnityEngine;

namespace DefaultNamespace.AnimationExamples
{
    public class MovingWithCoroutine : MonoBehaviour
    {
        public IEnumerator Move(float delay, float duration, float xOffset)
        {
            yield return new WaitForSeconds(delay);
            var finish = Time.time + duration;
            while (Time.time < finish)
            {
                transform.Translate(Vector3.right * (Time.deltaTime * xOffset) / duration);
                yield return null;
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(Move));
        }
    }
}