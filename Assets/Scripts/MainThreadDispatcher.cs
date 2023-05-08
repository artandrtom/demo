using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static MainThreadDispatcher _instance;

        private List<Action> _actions = new List<Action>();
        
        private void Awake()
        {
            if(_instance != null) return;
            _instance = this;
            DontDestroyOnLoad(_instance.gameObject);
        }

        public static void Post(Action action)
        {
            _instance._actions.Add(action);
        }
        
        private void Update()
        {
            var old = _actions.ToList();
            _actions = new List<Action>();
            foreach (Action action in old)
            {
                action?.Invoke();
            }
        }
    }
}