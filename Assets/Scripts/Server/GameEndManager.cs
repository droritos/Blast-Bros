using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using System.Linq;

namespace Game.Server
{
    [Serializable]
    public struct LeaderboardEntry
    {
        public string name;
        public int characterIdx;
        public double time;

        public override string ToString() => $"LeaderboardEntry({name}, {characterIdx}, {time})";
    }

    public class GameEndManager : NetworkBehaviour
    {
        [SerializeField] private CanvasGroup playerDiedMessageCanvas;

        public event Action<LeaderboardEntry[]> OnGameEnd;

        private readonly List<LeaderboardEntry> _leaderboards = new();
        internal void MarkPlayerAsDead(PlayerRef player, string playerName,int characterIdx)
        {
            Debug.Log($"[SERVER] Player {player} ({playerName}) had died at {Runner.SimulationTime}");
            var entry = new LeaderboardEntry
            {
                name = playerName,
                characterIdx = characterIdx,
                time = Runner.SimulationTime
            };
            Debug.Log($"$[SERVER] leaderboard entry: {entry}");
            _leaderboards.Add(entry);

            ShowPlayerDiedCanvasRPC(player);
            CheckIfAllDied();
        }

        private void CheckIfAllDied()
        {
            int maxPlayers = GetTrueMaxPlayers();

            if (!HasStateAuthority || _leaderboards.Count != maxPlayers)
            {
                return;
            }

            Debug.Log("[SERVER] All players have died");
            Debug.Log("[SERVER] Showing leaderboards...");

            // Sort by highest time first (best score at top)
            // CompareTo: otherEntry.time.CompareTo(entry.time) puts larger values first
            _leaderboards.Sort((entry, otherEntry) => otherEntry.time.CompareTo(entry.time));

            string[] players = _leaderboards.Select(x => x.name).ToArray();
            int[] characterIdxs = _leaderboards.Select(x => x.characterIdx).ToArray();
            double[] times = _leaderboards.Select(x => x.time).ToArray();
            NotifyGameEndRPC(players, characterIdxs, times);
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

        private IEnumerator DelayedShutdown()
        {
            Debug.Log("[CLIENT] Starting delayed shutdown...");
            yield return new WaitForSeconds(0.1f);
            Debug.Log("[CLIENT] Shutting down...");
            Runner.Shutdown();
        }

        #region Outbound RPCs
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void ShowPlayerDiedCanvasRPC([RpcTarget] PlayerRef player) => playerDiedMessageCanvas.ShowCanvasGroup();

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void NotifyGameEndRPC(string[] players, int[] characterIdxs, double[] times)
        {
            playerDiedMessageCanvas.HideCanvasGroup();

            // reconstruct leaderboard entries
            LeaderboardEntry[] entries = new LeaderboardEntry[players.Length];
            for (int i = 0; i < players.Length; i++)
            {
                entries[i] = new LeaderboardEntry
                {
                    name = players[i], characterIdx = characterIdxs[i], time = times[i]
                };
                Debug.Log($"[CLIENT] Reconstructed entry: {entries[i]}");
            }

            OnGameEnd?.Invoke(entries);

            StartCoroutine(DelayedShutdown());
        }
        #endregion
    }
}
