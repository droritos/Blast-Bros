using System;
using System.IO;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class GameSessionConfig
    {
        [SerializeField] private string sessionName = "TestSession";
        [SerializeField] [Range(0.1f, 10f)] private float retryDelaySeconds = 1f;
        [SerializeField] private string levelSceneName = "Level Scene";
        [SerializeField] [Range(1, 20)] private int maxRetryAttempts = 5;

        public string SessionName => sessionName;
        public float RetryDelaySeconds => retryDelaySeconds;
        public string LevelSceneName => levelSceneName;
        public int MaxRetryAttempts => maxRetryAttempts;

        public static GameSessionConfig GetCurrentSessionConfig(GameData gameData)
        {
            const string ConfigFileName = "game_config.json";
            var config = gameData.defaultGameSessionConfig;

            Debug.Log($"[GLOBAL] Looking at working directory: {Directory.GetCurrentDirectory()}");

            if (!File.Exists(ConfigFileName))
            {
                Debug.Log($"[GLOBAL] Config file not found at {ConfigFileName}, using default configuration");
                return config;
            }

            try
            {
                string configText = File.ReadAllText(ConfigFileName);

                if (string.IsNullOrWhiteSpace(configText))
                {
                    Debug.LogWarning("[GLOBAL] Config file is empty, using default configuration");
                    return config;
                }

                JsonUtility.FromJsonOverwrite(configText, config);
                Debug.Log("[GLOBAL] Successfully loaded custom game configuration");
                return config;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GLOBAL] Failed to load config file: {ex.Message}. Using default configuration.");
                return config;
            }
        }
    }
}
