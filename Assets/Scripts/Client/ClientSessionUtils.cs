using Fusion;
using Game.Data;
using Game.Server;
using UnityEngine;

namespace Game.Client
{
    public static class ClientSessionUtils
    {
        public static async Awaitable<bool> Connect(GameSessionConfig config, string playerName="Unknown")
        {
            int attempts = 0;
            NetworkRunner runner = null;
            StartGameResult result;

            do
            {
                attempts++;
                Debug.Log($"[CLIENT] Connection attempt {attempts}/{config.maxRetryAttempts}...");

                // Clean up previous runner if retry
                if (runner != null)
                {
                    Object.Destroy(runner.gameObject);
                    runner = null;
                }

                runner = CreateNetworkRunner("Network Client");
                var startGameArgs = new StartGameArgs
                {
                    GameMode = GameMode.Client,
                    SessionName = config.sessionName
                };

                result = await runner.StartGame(startGameArgs);
                Debug.Log($"[CLIENT] Result: {result.Ok}, Error: {result.ErrorMessage}");

                if (result.Ok)
                {
                    await HandleSuccessfulConnection(runner, playerName);
                    return true;
                }

                Debug.LogWarning($"[CLIENT] Attempt {attempts} failed: {result.ErrorMessage}");
                Object.Destroy(runner.gameObject);

                if (attempts < config.maxRetryAttempts)
                {
                    await Awaitable.WaitForSecondsAsync(config.retryDelaySeconds);
                }

            } while (attempts < config.maxRetryAttempts);

            Debug.LogError($"[CLIENT] Failed to connect after {config.maxRetryAttempts} attempts");
            return false;
        }

        private static async Awaitable HandleSuccessfulConnection(NetworkRunner runner, string playerName)
        {
            Debug.Log("[CLIENT] Successfully connected to session!");
            while (GameManager.instance?.Object?.IsValid != true)
            {
                Debug.Log("[CLIENT] Waiting for GameManager NetworkObject to be ready...");
                await Awaitable.WaitForSecondsAsync(0.1f);
            }

            GameManager.instance.RequestCreatePlayerObjectRPC(name: playerName);
        }

        private static NetworkRunner CreateNetworkRunner(string name)
        {
            var runnerGO = new GameObject(name);
            Object.DontDestroyOnLoad(runnerGO);
            return runnerGO.AddComponent<NetworkRunner>();
        }
    }
}
