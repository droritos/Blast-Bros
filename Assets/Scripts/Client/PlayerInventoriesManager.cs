using Fusion;
using System.Collections.Generic;
using UnityEngine;
using Game.Client;
using Game.Server;

public class PlayerInventoriesManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    private readonly Dictionary<PlayerRef, int> _playerBombCounts = new();

    public void PlayerJoined(PlayerRef player)
    {
        _playerBombCounts[player] = 1; // Start with 1 bomb
        UpdateBombCountClientRPC(player, _playerBombCounts[player]);
    }

    public void PlayerLeft(PlayerRef player)
    {
        _playerBombCounts.Remove(player);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RequestUseBombRPC(Vector3 position, RpcInfo info = default)
    {
        if (_playerBombCounts.TryGetValue(info.Source, out int count) && count > 0)
        {
            _playerBombCounts[info.Source]--;
            GameManagerRequestBroker.RequestBomb(info.Source, position); // Pass owner too
            UpdateBombCountClientRPC(info.Source, _playerBombCounts[info.Source]);
        }
        else
        {
            NotifyBombUseFailedRPC(info.Source);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void UpdateBombCountClientRPC([RpcTarget] PlayerRef player, int bombCount)
    {
        if (player == Runner.LocalPlayer)
        {
            PlayerScript.LocalPlayer?.Inventory?.SetCurrentBombCount(bombCount);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void NotifyBombUseFailedRPC([RpcTarget] PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            PlayerScript.LocalPlayer?.Inventory?.NotifyUseFailed();
        }
    }

    public void RestoreBomb(PlayerRef player)
    {
        if (_playerBombCounts.ContainsKey(player))
        {
            _playerBombCounts[player]++;
            UpdateBombCountClientRPC(player, _playerBombCounts[player]);
        }
    }
}
