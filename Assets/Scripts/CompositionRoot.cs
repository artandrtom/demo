using System;
using Networking.Webhooks;
using UnityEngine;

namespace DefaultNamespace
{
    public class CompositionRoot : MonoBehaviour
    {
        private void Start()
        {
            new WebhooksListener().Start();
        }
    }
}