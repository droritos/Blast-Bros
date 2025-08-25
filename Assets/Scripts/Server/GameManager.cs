using System.Collections;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class GameManager : NetworkBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private GridData gridData;


        [SerializeField] private PlayerInventoriesManager _playerInventoriesManager;

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
            bool isRequstSucceed = _playerInventoriesManager.TryConsumePlayerBombRPC(info);
            if (!isRequstSucceed)
                return;

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
            _playerInventoriesManager.RestoreBomb(playerRef);
            // Invoke RestoreBombCount to the user's inventory
            //RequestRestoreBomb(playerRef);
        }

        private void HitAndDestroyCrate(Vector3 origin, Vector3 direction)
        {
            Physics.Linecast(origin, origin + direction * 2f, out RaycastHit hit);
            if (!hit.collider) return;

            var networkObject = hit.collider.gameObject.GetComponent<NetworkObject>();
            if (hit.collider.CompareTag("Destructible"))
            {
                Runner.Despawn(networkObject);
            }
        }
    }
}
