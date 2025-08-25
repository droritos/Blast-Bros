using Fusion;
using Game.Client;
using Game.Server;
using System.Collections.Generic;
using UnityEngine;
using static PlasticGui.WorkspaceWindow.Merge.MergeInProgress;

public class PlayerInventoriesManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    private readonly Dictionary<PlayerRef, int> _playerBombCounts = new();

    private void OnEnable()
    {
        GameManagerRequestBroker.OnRestoreBomb += RestoreBomb;
    }
    private void OnDisable()
    {
        GameManagerRequestBroker.OnRestoreBomb -= RestoreBomb;
    }

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
        Debug.Log($"[SERVER] Bomb request from: {info.Source}");
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
            Debug.Log($"[Client] Bomb update to: {PlayerScript.LocalPlayer?.Inventory.CurrentBombCount}");
            PlayerScript.LocalPlayer?.Inventory?.SetCurrentBombCount(bombCount);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void NotifyBombUseFailedRPC([RpcTarget] PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Debug.Log($"No More Bombs ! Boom1!!!");
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
