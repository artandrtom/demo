using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace DefaultNamespace
{
    public class Examples : MonoBehaviour
    {
        private IEnumerator Start()
        {
            Debug.Log(Time.frameCount);
            yield return Coroutine();
            Debug.Log(Time.frameCount);
        }

        private void Update()
        {
            
        }

        private IEnumerator Coroutine()
        {
            if(false) yield return null;
        }

        private async Task SomeTask()
        {
            if(false) await Task.Yield();
        }
    }
}