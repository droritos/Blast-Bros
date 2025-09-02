using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class GameCharacterSpawner : NetworkBehaviour
    {
        [SerializeField] private List<Transform> positions = new();

        internal void SpawnCharacter(GameObject prefab, PlayerRef player)
        {
            if (positions.Count == 0)
            {
                Debug.LogWarning("[SERVER] No positions available to Spawn Character");
                return;
            }

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
