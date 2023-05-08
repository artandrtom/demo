using System.Collections;
using UnityEngine;

namespace DefaultNamespace.AnimationExamples
{
    public class MovingObjectsWithCoroutine : MonoBehaviour
    {
        [SerializeField] private GameObject _finish;
        [SerializeField] private MovingWithCoroutine[] _objects;
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(Animate());
            }
        }

        private IEnumerator Animate()
        {
            for (int i = 0; i < _objects.Length; i++)
            {
                yield return _objects[i].Move(0f, (float)(i + 1)/2, 10 + i);
            }
            _finish.SetActive(true);
        }
    }
}