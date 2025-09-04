using Game;
using Game.Client;
using Game.Client.Misc;
using Game.Data;
using Game.Server;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class ClientHostRoomUI : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private TMP_InputField playerName;
        [SerializeField] private TMP_Dropdown maxPlayersDropdown;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button randomizeNicknameButton;

        private bool _startedServer;

        private void OnEnable()
        {
            joinButton.onClick.AddListener(JoinGameButton);
            randomizeNicknameButton.onClick.AddListener(RandomizeNicknameButton);
        }

        private void OnDisable()
        {
            joinButton.onClick.RemoveListener(JoinGameButton);
            randomizeNicknameButton.onClick.RemoveListener(RandomizeNicknameButton);
        }

        private void JoinGameButton()
        {
            if (_startedServer) return;

            var sessionConfig = GameSessionConfig.GetCurrentSessionConfig(gameData);
            int numMaxPlayers = int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text.Replace(" Players", "").Replace(" Player", ""));
            sessionConfig.numMaxPlayers = numMaxPlayers;
            _ = ServerSessionUtils.StartHost(sessionConfig, playerName.text);

            _startedServer = true;
        }

        private void RandomizeNicknameButton() => playerName.text = SillyId.GenerateGamertag();
    }
}
