using System.Collections;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private GridData gridData;

        [SerializeField] public PlayerInventoriesManager playerInventoriesManager;
        [SerializeField] private GridManager gridManager;

        public static GameManager instance;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
        }

        public void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        public void RequestBombAtLocationRPC(Vector3 position, RpcInfo info = default)
        {
            bool didConsumeBomb = playerInventoriesManager.TryConsumePlayerBombRPC(info);
            if (!didConsumeBomb)
            {
                return;
            }

            Debug.Log("[Server] Placing bomb in location");
            position = gridData.AlignToClosestGridPosition(position);
            NetworkObject bombInstance = Runner.Spawn(gameData.bombPrefab, position);

            StartCoroutine(DoExplosion(position, bombInstance, info.Source));
        }

        IEnumerator DoExplosion(Vector3 bombPosition, NetworkObject bombInstance, PlayerRef playerRef)
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
                gridManager.DespawnGridItem(hit.collider.transform.position);
            }
        }
    }
}
