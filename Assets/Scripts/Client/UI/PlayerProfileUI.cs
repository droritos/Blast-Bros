using System;
using Fusion;
using Game.Server;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game
{
    public class PlayerProfileUI : NetworkBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameTMP;
        [SerializeField] private TextMeshProUGUI currentBombStatusTMP;
        [SerializeField] private Image characterIcon;

        public override void Spawned()
        {
            GameManager.instance.playerInventoriesManager.OnBombCountUpdated += OnBombCountUpdated;
            GameManager.instance.playerInventoriesManager.OnBombUseFailed += OnBombUseFailed;

            InitializeProfile("Me", null);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            GameManager.instance.playerInventoriesManager.OnBombCountUpdated -= OnBombCountUpdated;
            GameManager.instance.playerInventoriesManager.OnBombUseFailed -= OnBombUseFailed;
        }

        private void OnBombUseFailed() => Debug.Log("FAILED!!!!");

        private void OnBombCountUpdated(int currentBombs, int totalBombs) => UpdateBombStatus(currentBombs, totalBombs);

        private void InitializeProfile(string playerName, Sprite icon)
        {
            playerNameTMP.SetText(playerName);
            characterIcon.sprite = icon;
            UpdateBombStatus(1,1);
        }

        private void UpdateBombStatus(int currentBombs, int totalBombs) => currentBombStatusTMP.SetText($"{currentBombs}/{totalBombs}");
    }
}
