using System.Collections;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private GridData gridData;
        private void OnEnable()
        {
            GameManagerRequestBroker.OnRequestBomb += RequestBombAtLocationRPC;
        }

        private void OnDisable()
        {
            GameManagerRequestBroker.OnRequestBomb -= RequestBombAtLocationRPC;
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private void RequestBombAtLocationRPC(PlayerRef playerRef,Vector3 position)
        {
            Debug.Log("[Server] Placing bomb in location");
            position = gridData.AlignToClosestGridPosition(position);
            NetworkObject bombInstance = Runner.Spawn(gameData.bombPrefab, position);

            StartCoroutine(BombExplosionCoroutine(playerRef, position, bombInstance));
        }

        private IEnumerator BombExplosionCoroutine(PlayerRef playerRef,Vector3 bombPosition, NetworkObject bombInstance)
        {
            yield return new WaitForSeconds(0.5f);

            // 4-way raycasts to adjacent destructible blocks
            HitAndDestroyCrate(bombPosition, Vector3.forward);
            HitAndDestroyCrate(bombPosition, Vector3.back);
            HitAndDestroyCrate(bombPosition, Vector3.left);
            HitAndDestroyCrate(bombPosition, Vector3.right);

            yield return new WaitForSeconds(2f);

            Runner.Despawn(bombInstance);

            // Invoke RestoreBombCount to the user's inventory
            GameManagerRequestBroker.RequestRestoreBomb(playerRef);
        }

        private void HitAndDestroyCrate(Vector3 origin, Vector3 direction)
        {
            Physics.Linecast(origin, origin + direction * 2f, out RaycastHit hit);
            if(hit.collider && hit.collider.CompareTag("Destructible"))
                Runner.Despawn(hit.collider.gameObject.GetComponent<NetworkObject>());
        }
    }
}
