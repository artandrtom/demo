using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;

        private void Awake()
        {
            if(_instance != null) return;
            _instance = this;
            DontDestroyOnLoad(_instance.gameObject);
        }

        public static void Post(Action action)
        {
            
        }
    }
}