using Game;
using Game.Client;
using Game.Client.Misc;
using Game.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI
{
    public class ClientJoinRoomUI : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private TMP_InputField playerName;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button randomizeNicknameButton;

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
            var sessionConfig = GameSessionConfig.GetCurrentSessionConfig(gameData);
            _ = ClientSessionUtils.Connect(sessionConfig, playerName.text);
        }

        private void RandomizeNicknameButton() => playerName.text = SillyId.GenerateGamertag();
    }
}
