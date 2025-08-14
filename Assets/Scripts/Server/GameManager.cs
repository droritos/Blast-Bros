using System;
using System.Collections;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private GridData gridData;

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
        public void RequestBombAtLocationRPC(Vector3 position)
        {
            Debug.Log("[Server] Placing bomb in location");
            position = gridData.AlignToClosestGridPosition(position);
            var bombInstance = Runner.Spawn(gameData.bombPrefab, position);

            StartCoroutine(BombExplosionCoroutine(position, bombInstance));
        }

        private IEnumerator BombExplosionCoroutine(Vector3 bombPosition, NetworkObject bombInstance)
        {
            yield return new WaitForSeconds(0.5f);

            // 4-way raycasts to adjacent destructible blocks
            HitAndDestroyCrate(bombPosition, Vector3.forward);
            HitAndDestroyCrate(bombPosition, Vector3.back);
            HitAndDestroyCrate(bombPosition, Vector3.left);
            HitAndDestroyCrate(bombPosition, Vector3.right);

            yield return new WaitForSeconds(2f);

            Runner.Despawn(bombInstance);
        }

        private void HitAndDestroyCrate(Vector3 origin, Vector3 direction)
        {
            Physics.Linecast(origin, origin + direction * 2f, out RaycastHit hit);
            if (!hit.collider || !hit.collider.CompareTag("Destructible"))
            {
                return;
            }

            Runner.Despawn(hit.collider.gameObject.GetComponent<NetworkObject>());
        }
    }
}
