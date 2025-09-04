using System.Collections;
using Fusion;
using Game.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
    public class PlayerProfileUI : NetworkBehaviour
    {
        [SerializeField] private PlayerInventoriesManager playerInventoriesManager;
        [SerializeField] private TextMeshProUGUI playerNameTMP;
        [SerializeField] private TextMeshProUGUI currentBombStatusTMP;
        [SerializeField] private Image characterIcon;

        [SerializeField] private GameData gameData;

        private PlayerData _localPlayerData;

        public override void Spawned()
        {
            playerInventoriesManager.OnBombCountUpdated += OnBombCountUpdated;
            playerInventoriesManager.OnBombUseFailed += OnBombUseFailed;

            StartCoroutine(WaitForPlayerData());
        }

        private IEnumerator WaitForPlayerData()
        {
            var waitForPlayerData = new WaitForSeconds(0.1f);
            while (!Runner.GetPlayerObject(Runner.LocalPlayer))
            {
                yield return waitForPlayerData;
            }

            _localPlayerData = Runner.GetPlayerObject(Runner.LocalPlayer).GetComponent<PlayerData>();
            _localPlayerData.OnPlayerNameChanged += OnPlayerNameChanged;
            _localPlayerData.OnCharacterIndexChanged += OnCharacterIndexChanged;

            InitializeProfile();
        }

        private void InitializeProfile()
        {
            OnPlayerNameChanged(_localPlayerData.PlayerName.Value);
            OnCharacterIndexChanged(_localPlayerData.CharacterIndex);
            UpdateBombStatus(1, 1);
        }

        private void OnPlayerNameChanged(string newName) => playerNameTMP.SetText(newName);

        private void OnCharacterIndexChanged(int characterIdx) =>
            characterIcon.sprite =
                characterIdx < 0 ? null : gameData.characters[characterIdx].characterIcon;

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_localPlayerData)
            {
                _localPlayerData.OnPlayerNameChanged -= OnPlayerNameChanged;
                _localPlayerData.OnCharacterIndexChanged -= OnCharacterIndexChanged;
            }

            playerInventoriesManager.OnBombCountUpdated -= OnBombCountUpdated;
            playerInventoriesManager.OnBombUseFailed -= OnBombUseFailed;
        }

        private void OnBombUseFailed() => Debug.Log("FAILED!!!!"); // TODO: vfx?

        private void OnBombCountUpdated(int currentBombs, int totalBombs) => UpdateBombStatus(currentBombs, totalBombs);

        private void UpdateBombStatus(int currentBombs, int totalBombs) =>
            currentBombStatusTMP.SetText($"{currentBombs}/{totalBombs}");
    }
}
