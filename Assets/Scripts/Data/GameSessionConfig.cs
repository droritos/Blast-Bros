using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(fileName = "GameSessionConfig", menuName = "Game/Game Session Config")]
    public class GameSessionConfig : ScriptableObject
    {
        [SerializeField] private string sessionName = "TestSession";
        [SerializeField, Range(0.1f, 10f)] private float retryDelaySeconds = 1f;
        [SerializeField] private string levelSceneName = "Level Scene";
        [SerializeField, Range(1, 20)] private int maxRetryAttempts = 5;

        public string SessionName => sessionName;
        public float RetryDelaySeconds => retryDelaySeconds;
        public string LevelSceneName => levelSceneName;
        public int MaxRetryAttempts => maxRetryAttempts;
    }
}
