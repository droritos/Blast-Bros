using System.Linq;
using Game.Client.Misc;
using Game.Data;
using Unity.Multiplayer.Playmode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameSessionStarter : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private bool shouldRedirectToMenu;

        private async void Start()
        {
            var sessionConfig = GameSessionConfig.GetCurrentSessionConfig(gameData);
#if UNITY_EDITOR
            await StartEditorSession(sessionConfig);
#elif UNITY_SERVER
            await StartDedicatedServer(sessionConfig);
#else
            await StartClientSession(sessionConfig);
#endif
        }

#if UNITY_EDITOR
        private async Awaitable StartEditorSession(GameSessionConfig sessionConfig)
        {
            var playerTags = CurrentPlayer.ReadOnlyTags().ToHashSet();

            if (playerTags.Contains("Server"))
            {
                await StartDedicatedServer(sessionConfig);
            }
            else if (playerTags.Contains("Host"))
            {
                await StartHostModeSession(sessionConfig);
            }
            else
            {
                await StartClientSession(sessionConfig);
            }
        }
#endif
        private async Awaitable StartHostModeSession(GameSessionConfig sessionConfig) => await Server.ServerSessionUtils.StartHost(sessionConfig);

        private async Awaitable StartDedicatedServer(GameSessionConfig sessionConfig) => await Server.ServerSessionUtils.StartServer(sessionConfig);

        private async Awaitable StartClientSession(GameSessionConfig sessionConfig)
        {
            if(shouldRedirectToMenu)
            {
                SceneManager.LoadScene("Main Menu");
            }
            else
            {
                string playerName = SillyId.GenerateGamertag();
                await Client.ClientSessionUtils.Connect(sessionConfig, playerName: playerName);
            }
        }
    }
}
