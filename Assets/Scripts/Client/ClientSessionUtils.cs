using Fusion;
using Game.Data;
using UnityEngine;

namespace Game.Client
{
    public static class ClientSessionUtils
    {
        public static async Awaitable<bool> Connect(GameSessionConfig config)
        {
            int attempts = 0;
            NetworkRunner runner = null;
            StartGameResult result;

            do
            {
                attempts++;
                Debug.Log($"[CLIENT] Connection attempt {attempts}/{config.MaxRetryAttempts}...");

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
                    SessionName = config.SessionName
                };

                result = await runner.StartGame(startGameArgs);
                Debug.Log($"[CLIENT] Result: {result.Ok}, Error: {result.ErrorMessage}");

                if (result.Ok)
                {
                    Debug.Log("[CLIENT] Successfully connected to session!");
                    return true;
                }

                Debug.LogWarning($"[CLIENT] Attempt {attempts} failed: {result.ErrorMessage}");
                Object.Destroy(runner.gameObject);

                if (attempts < config.MaxRetryAttempts)
                {
                    await Awaitable.WaitForSecondsAsync(config.RetryDelaySeconds);
                }

            } while (attempts < config.MaxRetryAttempts);

            Debug.LogError($"[CLIENT] Failed to connect after {config.MaxRetryAttempts} attempts");
            return false;
        }

        private static NetworkRunner CreateNetworkRunner(string name)
        {
            var runnerGO = new GameObject(name);
            Object.DontDestroyOnLoad(runnerGO);
            return runnerGO.AddComponent<NetworkRunner>();
        }
    }
}
