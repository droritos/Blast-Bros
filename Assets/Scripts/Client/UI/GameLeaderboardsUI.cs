using System;
using System.Linq;
using Game.Server;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.Client.UI
{
    public class GameLeaderboardsUI : MonoBehaviour
    {
        [SerializeField] private GameEndManager gameEndManager;
        [SerializeField] private CanvasGroup leaderboardsCanvas;
        [SerializeField] private GameData gameData;

        [Header("UI")] //
        [SerializeField] private GameLeaderboardsPlayerSpot winnerSpot;
        [SerializeField] private GameLeaderboardsPlayerSpot spotTemplate;
        [SerializeField] private RectTransform losersSectionRoot;
        [SerializeField] private Button backToMenuButton;

        private void OnEnable()
        {
            gameEndManager.OnGameEnd += RenderLeaderboards;
            backToMenuButton.onClick.AddListener(BackToMenu);
        }

        private void OnDisable()
        {
            gameEndManager.OnGameEnd -= RenderLeaderboards;
            backToMenuButton.onClick.RemoveListener(BackToMenu);
        }

        private void BackToMenu() => SceneManager.LoadScene("Main Menu");

        private void RenderLeaderboards(LeaderboardEntry[] results)
        {
            leaderboardsCanvas.alpha = 1;
            leaderboardsCanvas.blocksRaycasts = true;
            leaderboardsCanvas.interactable = true;

            Array.Sort(results, (entry, otherEntry) => entry.time.CompareTo(otherEntry.time));
            RenderWinner(results);

            foreach (var otherEntry in results.Skip(1))
            {
                RenderOtherEntry(otherEntry);
            }
        }

        private void RenderOtherEntry(LeaderboardEntry entry)
        {
            Debug.Log($"[entry] {entry.name}, {entry.characterIdx}, {entry.time}");
            var spotInstance = Instantiate(spotTemplate, losersSectionRoot);
            spotInstance.UpdateDetails(entry.name, gameData.characters[entry.characterIdx].characterIcon);
        }

        private void RenderWinner(LeaderboardEntry[] results)
        {
            var winner = results[0];
            Debug.Log($"[entry] {winner.name}, {winner.characterIdx}, {winner.time}");
            winnerSpot.UpdateDetails(winner.name, gameData.characters[winner.characterIdx].characterIcon);
        }
    }
}
