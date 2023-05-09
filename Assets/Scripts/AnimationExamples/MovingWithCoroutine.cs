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
            float time = 0f;
            float progress = 0f;
            var initial = transform.position;
            var targetPos = new Vector3(initial.x + xOffset, initial.y, initial.z);
            while (progress < 1f)
            {
                progress = time / duration;
                transform.position = Vector3.Lerp(initial, targetPos, progress);
                yield return null;
                time += Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(Move));
        }
    }
}