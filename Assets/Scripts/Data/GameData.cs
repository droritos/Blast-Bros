using Game.Data;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "GameData.asset", menuName = "Game/GameData")]
    public class GameData : ScriptableObject
    {
        public ExplosionEffectSettings explosionEffectSettings;
        public GameSessionConfig defaultGameSessionConfig;

        [Header("Prefabs")]
        public CharacterData[] characters;
        public GameObject bombPrefab;
    }
}
