using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class GameCharacterSpawner : NetworkBehaviour
    {
        [SerializeField] private List<Transform> positions = new();
        private readonly List<(GameObject, PlayerRef)> _characterRequests = new();

        internal void RequestCharacterSpawnAtReady(GameObject prefab, PlayerRef player)
        {
            if (positions.Count == _characterRequests.Count)
            {
                Debug.LogWarning("[SERVER] No positions available to Spawn Character");
                return;
            }

            _characterRequests.Add((prefab, player));
        }

        internal void SpawnAllCharacters()
        {
            Debug.Log("[SERVER] Spawning all player characters");
            foreach (var (prefab, player) in _characterRequests)
            {
                SpawnCharacter(prefab, player);
            }
        }

        private void SpawnCharacter(GameObject prefab, PlayerRef player)
        {
            var position = positions[^1].position;
            positions.RemoveAt(positions.Count - 1);

            NetworkObject physicalGameObject = Runner.Spawn(prefab, position: position, inputAuthority: player);
            var playerObject = Runner.GetPlayerObject(player);
            var playerData = playerObject.GetComponent<PlayerData>();
            playerData.UpdatePhysicalPlayerObject(physicalGameObject);
        }

        private void Awake()
        {
            // shuffle spawn points
            var random = new  System.Random();
            for(int i = 0; i < positions.Count - 1; i++)
            {
                int pos = random.Next(i, positions.Count);
                (positions[i], positions[pos]) = (positions[pos], positions[i]);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (positions.Count == 0)
            {
                GameObject root = GameObject.Find("Spawn Positions");
                if (!root) return;

                foreach (Transform t in root.transform)
                {
                    positions.Add(t);
                }
            }
        }
#endif
    }
}
