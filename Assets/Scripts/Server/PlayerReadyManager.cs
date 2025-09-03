using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Game.Server
{
    public class PlayerReadyManager : NetworkBehaviour, IPlayerLeft
    {
        [SerializeField] private CanvasGroup readyCanvasGroup;

        private readonly Dictionary<PlayerRef, bool> _readyState = new();

        public event Action OnAllPlayersReady;

        public void PlayerLeft(PlayerRef player)
        {
            if (!HasStateAuthority) return;
            Debug.Log($"[SERVER] Player {player} left, the spot is now unmarked");
            _readyState.Remove(player);
        }

        public void MarkPlayerReady(PlayerRef player)
        {
            if (!HasStateAuthority) return;

            Debug.Log($"[SERVER] Marked {player} as ready ({_readyState.Count + 1}/{GetTrueMaxPlayers()})");
            _readyState.Add(player, true);
            ShowReadyCanvasForPlayerRPC(player);
            CheckIfAllAreReady();
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void ShowReadyCanvasForPlayerRPC([RpcTarget] PlayerRef player) => ShowReadyCanvas();

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void HideReadyCanvasForAllPlayersRPC() => HideReadyCanvas();

        private void ShowReadyCanvas()
        {
            readyCanvasGroup.alpha = 1;
            readyCanvasGroup.blocksRaycasts = true;
            readyCanvasGroup.interactable = true;
        }

        private void HideReadyCanvas()
        {
            readyCanvasGroup.alpha = 0;
            readyCanvasGroup.blocksRaycasts = false;
            readyCanvasGroup.interactable = false;
        }

        private void CheckIfAllAreReady()
        {
            int maxPlayers = GetTrueMaxPlayers();

            if (!HasStateAuthority || _readyState.Count != maxPlayers)
            {
                return;
            }

            Debug.Log("[SERVER] All players ready");
            Debug.Log("[SERVER] Closing session to new players");
            Runner.SessionInfo.IsOpen = false;

            OnAllPlayersReady?.Invoke();
            HideReadyCanvasForAllPlayersRPC();
        }

        private int GetTrueMaxPlayers()
        {
            int maxPlayers = Runner.SessionInfo.MaxPlayers;
            if (Runner.GameMode == GameMode.Server)
            {
                maxPlayers -= 1;
            }

            return maxPlayers;
        }
    }
}
