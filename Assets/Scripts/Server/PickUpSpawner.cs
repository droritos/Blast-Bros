using System;
using Fusion;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Server
{
    public class PickupSpawner : NetworkBehaviour
    {
        [SerializeField] private NetworkObject pickUpPrefab;
        [SerializeField] private float chanceToCreate = 0.15f;
        public event Action<PlayerRef> OnPickUpCollected;

        public void TrySpawnPickup(Vector3 position)
        {
            if (Random.value > chanceToCreate)
            {
                return;
            }

            var pickupGameObject = Runner.Spawn(pickUpPrefab, position, Quaternion.identity);
            pickupGameObject.transform.SetParent(transform);

            var pickUp = pickupGameObject.GetComponent<PickUpScript>();
            pickUp.OnPickUpCollected += OnPickUpCollected;
        }
    }
}
