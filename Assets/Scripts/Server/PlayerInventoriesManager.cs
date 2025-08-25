using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Server
{
    public class PlayerInventoriesManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        private readonly Dictionary<PlayerRef, int> _playerBombCounts = new();

        public int LocalPlayerCurrentBombCount { get; private set; }
        public int LocalPlayerMaxBombCount { get; private set; }

        public void PlayerJoined(PlayerRef player)
        {
            _playerBombCounts[player] = 1; // Start with 1 bomb
            UpdateBombCountClientRPC(player, _playerBombCounts[player]);
        }

        public void PlayerLeft(PlayerRef player)
        {
            _playerBombCounts.Remove(player);
        }
        public bool TryConsumePlayerBombRPC(RpcInfo info = default)
        {
            Debug.Log($"[SERVER] Bomb request from: {info.Source}");
            if (_playerBombCounts.TryGetValue(info.Source, out int count) && count > 0) // Handle 
            {
                _playerBombCounts[info.Source]--;
                UpdateBombCountClientRPC(info.Source, _playerBombCounts[info.Source]);

                return true;
            }
            else
            {
                NotifyBombUseFailedRPC(info.Source);
                return true;
            }
        }
        public void RestoreBomb(PlayerRef player) // Also Update Just UI?
        {
            if (_playerBombCounts.ContainsKey(player))
            {
                _playerBombCounts[player]++;
                UpdateBombCountClientRPC(player, _playerBombCounts[player]);
            }
        }

        public void RequestLocalPlayerData(out int currentBomb, out int maxBomb)
        {
            currentBomb = LocalPlayerCurrentBombCount;
            maxBomb = LocalPlayerMaxBombCount;
        }
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void UpdateBombCountClientRPC([RpcTarget] PlayerRef player, int bombCount) // Also Update Just UI?
        {
            if (player == Runner.LocalPlayer)
            {
                Debug.Log($"[Client] Bomb update to: {bombCount}");
                LocalPlayerCurrentBombCount = bombCount;
                //PlayerScript.LocalPlayer?.Inventory?.SetCurrentBombCount(bombCount);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void NotifyBombUseFailedRPC([RpcTarget] PlayerRef player) // Update Only UI?
        {
            if (player == Runner.LocalPlayer)
            {
                Debug.Log($"No More Bombs ! No Boom!!!!");
                //PlayerScript.LocalPlayer?.Inventory?.NotifyUseFailed();
            }
        }
    }
}
