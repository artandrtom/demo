using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    public class FpsCounter : MonoBehaviour
    {
        [SerializeField] private TMP_Text _fps;
        
        private float _count;
        private readonly WaitForSeconds _delay = new(0.1f);
        
        private IEnumerator Start()
        {
            while (true)
            {
                _count = 1f / Time.unscaledDeltaTime;
                _fps.text = $"FPS: {Mathf.Round(_count)}";
                yield return _delay;
            }
        }

        private void OnDestroy()
        {
            StopCoroutine(nameof(Start));
        }
    }
}