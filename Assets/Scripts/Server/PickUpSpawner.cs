using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Server
{
    public class PickupSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject pickUpPrefab;
        [SerializeField] private float chanceToCreate = 0.15f;
        [SerializeField] private float pickupRadius = 1.5f;

        private readonly List<NetworkObject> _pickUpObjects = new();
        public event Action<PlayerRef> OnPickUpCollected;

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
            {
                return;
            }

            _pickUpObjects.RemoveAll(pickup =>
            {
                foreach (var player in Runner.ActivePlayers)
                {
                    var playerObj = Runner.GetPlayerObject(player);
                    if (!playerObj ||
                        Vector3.Distance(playerObj.transform.position, pickup.transform.position) > pickupRadius)
                    {
                        continue;
                    }

                    Debug.Log($"[SERVER] Player {player} collected a pickup.");
                    OnPickUpCollected?.Invoke(player);
                    Runner.Despawn(pickup);
                    return true; // Remove this pickup
                }

                return false; // Keep this pickup
            });
        }

        public void TrySpawnPickup(Vector3 position)
        {
            if (Random.value > chanceToCreate)
            {
                return;
            }

            var pickUp = Runner.Spawn(pickUpPrefab, position, Quaternion.identity);
            pickUp.transform.SetParent(transform);
            _pickUpObjects.Add(pickUp);
        }
    }
}
