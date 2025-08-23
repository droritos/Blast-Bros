using System.Linq;
using Fusion;
using Unity.Multiplayer.Playmode;
using UnityEngine;

namespace Game
{
    public class GameSessionStarter : MonoBehaviour
    {
        private const string TestSessionName = "TestSession";
        private const float RetryDelaySeconds = 1f;
        private const string LevelSceneName = "Level Scene";

        [SerializeField] private NetworkRunner networkRunnerPrefab;

        private async void Start()
        {
            var gameMode = GetGameMode();
            Debug.Log($"Configuration for player: {gameMode}");

            if (gameMode == GameMode.Client)
            {
                await RunClientMode();
            }
            else
            {
                await RunHostOrServerMode(gameMode);
            }
        }

        private static GameMode GetGameMode()
        {
#if UNITY_EDITOR
            var tags = CurrentPlayer.ReadOnlyTags().ToHashSet();
            return tags.Contains("Server") ? GameMode.Server :
                tags.Contains("Host") ? GameMode.Host :
                GameMode.Client;
#elif UNITY_SERVER
            return GameMode.Server;
#else
            return GameMode.Client;
#endif
        }

        private async Awaitable RunClientMode()
        {
            var startGameArgs = new StartGameArgs { GameMode = GameMode.Client, SessionName = TestSessionName };
            StartGameResult result;

            do
            {
                var runner = Instantiate(networkRunnerPrefab);

                Debug.Log("Attempting connection...");
                result = await runner.StartGame(startGameArgs);
                Debug.Log($"Result: {result.Ok}, Error: {result.ErrorMessage}, Shutdown: {result.ShutdownReason}");

                if (result.Ok)
                {
                    continue;
                }

                Destroy(runner);
                await Awaitable.WaitForSecondsAsync(RetryDelaySeconds);
            } while (!result.Ok);
        }

        private async Awaitable RunHostOrServerMode(GameMode gameMode)
        {
            var startGameArgs = new StartGameArgs { GameMode = gameMode, SessionName = TestSessionName };
            var runner = Instantiate(networkRunnerPrefab);

            var result = await runner.StartGame(startGameArgs);
            if (!result.Ok)
            {
                Debug.LogError($"Failed to start {gameMode}: {result.ErrorMessage}");
                Destroy(runner);
                Application.Quit();
                return;
            }

            await runner.LoadScene(LevelSceneName);
        }
    }
}
