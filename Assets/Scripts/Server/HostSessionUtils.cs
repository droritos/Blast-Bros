using Fusion;
using Game.Data;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Server
{
    public static class HostSessionUtils
    {
        public static async Awaitable<bool> StartHost(GameSessionConfig config) => await CreateSession(GameMode.Host, config);

        public static async Awaitable<bool> StartServer(GameSessionConfig config) => await CreateSession(GameMode.Server, config);

        private static async Awaitable<bool> CreateSession(GameMode gameMode, GameSessionConfig config)
        {
            var startGameArgs = new StartGameArgs
            {
                GameMode = gameMode,
                SessionName = config.SessionName
            };

            var runner = CreateNetworkRunner($"Network {gameMode}");
            string modeName = gameMode.ToString().ToUpper();

            Debug.Log($"[{modeName}] Starting session '{config.SessionName}'...");

            var result = await runner.StartGame(startGameArgs);
            if (!result.Ok)
            {
                Debug.LogError($"[{modeName}] Failed to start: {result.ErrorMessage}");
                Object.Destroy(runner.gameObject);
                return false;
            }

            Debug.Log($"[{modeName}] Session started successfully");

            bool sceneLoaded = await LoadGameScene(runner, config.LevelSceneName, gameMode);
            if (!sceneLoaded)
            {
                Debug.LogError($"[{modeName}] Failed to load scene");
                Object.Destroy(runner.gameObject);
                return false;
            }

            Debug.Log($"[{modeName}] Ready and running!");
            GameManager.instance.RequestCreatePlayerObjectRPC($"{config.SessionName}{gameMode}");
            return true;
        }

        private static NetworkRunner CreateNetworkRunner(string name)
        {
            var runnerGO = new GameObject(name);
            Object.DontDestroyOnLoad(runnerGO);
            return runnerGO.AddComponent<NetworkRunner>();
        }

        private static async Awaitable<bool> LoadGameScene(NetworkRunner runner, string sceneName, GameMode gameMode)
        {
            try
            {
                Debug.Log($"[{gameMode.ToString().ToUpper()}] Loading scene: {sceneName}");
                await runner.LoadScene(sceneName);
                Debug.Log($"[{gameMode.ToString().ToUpper()}] Scene loaded successfully");
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[{gameMode.ToString().ToUpper()}] Failed to load scene '{sceneName}': {ex.Message}");
                return false;
            }
        }
    }
}
