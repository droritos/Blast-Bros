using System.Linq;
using Fusion;
using Game.Data;
using Unity.Multiplayer.Playmode;
using UnityEngine;

namespace Game
{
    public class GameSessionStarter : MonoBehaviour
    {
        private const string TestSessionName = "TestSession";
        private const float RetryDelaySeconds = 1f;
        private const string LevelSceneName = "Level Scene";

        [SerializeField] private GameSessionConfig sessionConfig;

        private async void Start()
        {
#if UNITY_EDITOR
            var gameMode = GetGameMode();
            Debug.Log($"Configuration for player: {gameMode}");

            if (gameMode == GameMode.Client)
            {
                await Game.Client.ClientSessionUtils.Connect(sessionConfig);
            }
            else if (gameMode == GameMode.Server)
            {
                await Game.Server.HostSessionUtils.StartServer(sessionConfig);
            }
            else
            {
                await Game.Server.HostSessionUtils.StartHost(sessionConfig);
            }
#elif UNITY_SERVER
            await Game.Server.HostSessionUtils.StartServer(sessionConfig);
#else
            await Game.Client.ClientSessionUtils.Connect(sessionConfig);
#endif
        }

#if UNITY_EDITOR
        private static GameMode GetGameMode()
        {
            var tags = CurrentPlayer.ReadOnlyTags().ToHashSet();
            return tags.Contains("Server") ? GameMode.Server :
                tags.Contains("Host") ? GameMode.Host :
                GameMode.Client;
        }
#endif
    }
}
