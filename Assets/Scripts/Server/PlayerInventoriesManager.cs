using System;
using Fusion;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Server
{
    public class PlayerInventory
    {
        public int currentBombs = 1;
        public int maxBombs = 1;
    }

    public class PlayerInventoriesManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
    {
        private readonly Dictionary<PlayerRef, PlayerInventory> _playerBombCounts = new();

        public event Action<int, int> OnBombCountUpdated;
        public event Action OnBombUseFailed;

        public void PlayerJoined(PlayerRef player)
        {
            _playerBombCounts[player] = new PlayerInventory(); // Every player starts with one bomb
            UpdateBombCountClient(player);
        }

        public void PlayerLeft(PlayerRef player) => _playerBombCounts.Remove(player);

        #region Bomb Management
        internal bool TryConsumePlayerBombRPC(RpcInfo info = default)
        {
            Debug.Log($"[SERVER] Bomb request from: {info.Source}");
            if (_playerBombCounts.TryGetValue(info.Source, out var inventory) && inventory.currentBombs > 0) // Handle
            {
                _playerBombCounts[info.Source].currentBombs--;
                UpdateBombCountClient(info.Source);

                return true;
            }

            NotifyBombUseFailedRPC(info.Source);
            return false;
        }

        internal void RestoreBomb(PlayerRef player)
        {
            if (_playerBombCounts.TryGetValue(player, out var playerInventory))
            {
                playerInventory.currentBombs++;
                UpdateBombCountClient(player);
            }
        }

        internal void IncreaseBombCapacity(PlayerRef player)
        {
            if (_playerBombCounts.TryGetValue(player, out var playerInventory))
            {
                playerInventory.maxBombs++;
                UpdateBombCountClient(player);
            }
        }
        #endregion

        private void UpdateBombCountClient(PlayerRef source) => UpdateBombCountClientRPC(source, _playerBombCounts[source].currentBombs, _playerBombCounts[source].maxBombs);

        #region Outbound RPCs
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void UpdateBombCountClientRPC([RpcTarget] PlayerRef player, int bombCount, int maxBombs)
        {
            Debug.Log($"[Server] Bombs for player {player} update to: {bombCount}");
            OnBombCountUpdated?.Invoke(bombCount, 1); // TODO: max == 1;
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void NotifyBombUseFailedRPC([RpcTarget] PlayerRef player)
        {
            Debug.Log($"No More Bombs ! No Boom!!!!");
            OnBombUseFailed?.Invoke();
        }
        #endregion
    }
}
