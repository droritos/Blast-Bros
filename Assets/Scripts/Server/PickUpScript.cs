using System;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    [RequireComponent(typeof(Collider))]
    public class PickUpScript : NetworkBehaviour
    {
        public event Action<PlayerRef> OnPickUpCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (HasStateAuthority && other.CompareTag("Player"))
            {
                var player = other.GetComponent<NetworkObject>().InputAuthority;
                Debug.Log($"[SERVER] Player {player} collected a pickup.");
                OnPickUpCollected?.Invoke(player);
                Runner.Despawn(Object);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }
#endif
    }
}
