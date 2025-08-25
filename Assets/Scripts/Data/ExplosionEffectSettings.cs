using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "ExplosionVFXSettings.asset", menuName = "Game/Explosion VFX Settings")]
    public class ExplosionEffectSettings : ScriptableObject
    {
        [SerializeField] public float duration = 5f;
        [SerializeField] public float explosionDuration = 0.2f;
        [SerializeField] public float wiggleAngle = 10f, wiggleStep = 0.2f;
        [SerializeField] public float shrinkScale = 0.8f, shrinkTime = 0.1f;
        [SerializeField] public float popScale = 1.5f, popTime = 0.2f;
    }
}
