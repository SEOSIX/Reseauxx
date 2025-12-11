using System;
using Unity.Netcode;
using UnityEngine;

namespace DefaultNamespace
{
    public class Ball : NetworkBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == gameObject)
                return;
            if (other.gameObject.CompareTag("Hidder"))
            {
                var playerNet = other.gameObject.GetComponent<PlayerNetwork>();
                if (playerNet != null && playerNet.IsSpawned)
                {
                    playerNet.TakeDamageServerRpc(10);
                    LifeManager.instance.CheckLifePlayer();
                }
            }
        }
    }
}