using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DefaultNamespace.AnimationExamples
{
    public class MovingObjectsWithTask : MonoBehaviour
    {
        [SerializeField] private GameObject _finish;
        [SerializeField] private MovingWithTask[] _objects;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Animate();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                Cancel();
            }
        }

        private async void Animate()
        {
            for (int i = 0; i < _objects.Length; i++)
            {
                await _objects[i].Move(0f, (float)(i + 1)/2, 10 + i);
            }
            
            _finish.SetActive(true);
        }

        /*private async void TestException()
        {

        }

        private async Task TestExceptionTask()
        {

        }*/
        
        private void Cancel()
        {
            
        }
    }
}