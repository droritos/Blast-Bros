using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private GridData gridData;

        [Header("Manager references")] //
        [SerializeField] private PlayerReadyManager playerReadyManager;
        [SerializeField] private PlayerInventoriesManager playerInventoriesManager;
        [SerializeField] private GridManager gridManager;
        [SerializeField] private PickupSpawner pickupSpawner;
        [SerializeField] private GameCharacterSpawner characterSpawner;

        public static GameManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            pickupSpawner.OnPickUpCollected += playerInventoriesManager.IncreaseBombCapacity;
            playerReadyManager.OnAllPlayersReady += characterSpawner.SpawnAllCharacters;
        }

        public void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            pickupSpawner.OnPickUpCollected -= playerInventoriesManager.IncreaseBombCapacity;
            playerReadyManager.OnAllPlayersReady -= characterSpawner.SpawnAllCharacters;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RequestBombAtLocationRPC(Vector3 position, RpcInfo info = default)
        {
            bool didConsumeBomb = playerInventoriesManager.TryConsumePlayerBombRPC(info);
            if (!didConsumeBomb)
            {
                return;
            }

            Debug.Log("[SERVER] Placing bomb in location");
            position = gridData.AlignToClosestGridPosition(position);
            NetworkObject bombInstance = Runner.Spawn(gameData.bombPrefab, position);

            StartCoroutine(DoExplosion(position, bombInstance, info.Source));
        }

        private IEnumerator DoExplosion(Vector3 bombPosition, NetworkObject bombInstance, PlayerRef playerRef)
        {
            yield return new WaitForSeconds(gameData.explosionEffectSettings.TotalDuration + 0.1f);

            // 4-way raycasts to adjacent destructible blocks
            HitAndDestroyCrate(bombPosition, Vector3.forward);
            HitAndDestroyCrate(bombPosition, Vector3.back);
            HitAndDestroyCrate(bombPosition, Vector3.left);
            HitAndDestroyCrate(bombPosition, Vector3.right);

            Runner.Despawn(bombInstance);
            playerInventoriesManager.RestoreBomb(playerRef);
        }

        private void HitAndDestroyCrate(Vector3 origin, Vector3 direction)
        {
            Physics.Linecast(origin, origin + direction * 2f, out RaycastHit hit);
            if (!hit.collider) return;

            if (hit.collider.CompareTag("Destructible"))
            {
                var hitPosition = hit.collider.transform.position;
                pickupSpawner.TrySpawnPickup(hitPosition);
                gridManager.DespawnGridItem(hitPosition);
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RequestCreatePlayerObjectRPC(string name = "Unknown", RpcInfo info = default)
        {
            var player = info.Source;
            var sessionPrefab = Resources.Load<GameObject>("PlayerData");

            var playerDataGameObject = Runner.Spawn(sessionPrefab, inputAuthority: player);
            Runner.SetPlayerObject(player, playerDataGameObject);
            Debug.Log($"[GLOBAL] Spawned {playerDataGameObject} for {player}");

            var playerData = playerDataGameObject.GetComponent<PlayerData>();
            playerData.UpdatePlayerName(name);
        }
    }
}
