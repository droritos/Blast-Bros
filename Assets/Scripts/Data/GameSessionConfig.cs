using System;
using System.IO;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public class GameSessionConfig
    {
        [SerializeField] public string sessionName = "TestSession";
        [SerializeField] [Range(0.1f, 10f)] public float retryDelaySeconds = 1f;
        [SerializeField] public string levelSceneName = "Level Scene";
        [SerializeField] [Range(1, 20)] public int maxRetryAttempts = 5;
        [SerializeField] public int numMaxPlayers = 4;

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
