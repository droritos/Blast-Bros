    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fusion;
    using UnityEngine;
    using Random = UnityEngine.Random;

    namespace Game.Server
    {
        public class PickUpManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
        {
            public event Action<PlayerRef> OnPickUpCollected;
            private readonly Dictionary<PlayerRef, int> _playerCounts = new(); // Could be a List

            [Header("Pick Up Data")]
            [SerializeField] private NetworkObject _pickUpPrefab;
            [SerializeField] private float _chanceToCreate = 0.15f;
            [SerializeField] float _pickupRadius = 1.5f;

            private List<NetworkObject> _pickUpObjects = new List<NetworkObject>();

            public override void FixedUpdateNetwork()
            {
                base.FixedUpdateNetwork();
                List<NetworkObject> toRemove = new();

                foreach (var pickup in _pickUpObjects)
                {
                    foreach (var player in Runner.ActivePlayers)
                    {
                        var playerObj = Runner.GetPlayerObject(player);
                        if (playerObj == null) continue;

                        if (Vector3.Distance(playerObj.transform.position, pickup.transform.position) <= _pickupRadius)
                        {
                            RegisterPickup(player); // just trigger event or logic
                            toRemove.Add(pickup);   // mark for cleanup
                            break;
                        }
                    }
                }

                foreach (var pickup in toRemove)
                {
                    Runner.Despawn(pickup);
                    _pickUpObjects.Remove(pickup);
                }
            }

            public void PlayerJoined(PlayerRef player) => _playerCounts[player] = player.PlayerId;

            public void PlayerLeft(PlayerRef player) => _playerCounts.Remove(player);

#region Outbound RPCs
            public NetworkObject CreatePickUp(Vector3 position)
            {
                if (Random.value > _chanceToCreate)
                    return null; // Failed to create

                var pickUp = Runner.Spawn(_pickUpPrefab, position, Quaternion.identity);
                pickUp.transform.SetParent(transform);
                _pickUpObjects.Add(pickUp);
                return pickUp;
            }

            public void RegisterPickup(PlayerRef playerRef)
            {
                Debug.Log($"[Server] Player {playerRef} collected a pickup.");
                OnPickUpCollected?.Invoke(playerRef);

            }
#endregion
        }
    }
