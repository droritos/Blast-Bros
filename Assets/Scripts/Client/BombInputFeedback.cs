using Game.Server;
using UnityEngine;

namespace Game.Client
{
    public class BombInputFeedback : MonoBehaviour
    {
        [SerializeField] private ExplosionEffectSettings _explosionEffectSettings;
        [SerializeField] private GameObject _fakeBombPrefab;

        private float _cooldown => _explosionEffectSettings.TotalDuration;
        private float _elapsedTime;
        private bool _inputHappened = false;

        private void Update()
        {
            _elapsedTime += Time.deltaTime;

            if (!_inputHappened) return;

            if (_elapsedTime >= _cooldown + 0.1f)
            {
                PlaceFakeBomb();
                _inputHappened = false;
            }
        }

        public void TryPlaceFakeBomb()
        {
            // Reject if cooldown not done yet
            if (_elapsedTime < _cooldown + 0.1f) return;

            _inputHappened = true;
        }

        private void PlaceFakeBomb()
        {
            Vector3 alignedPosition = GameManager.instance.AlignToClosestGridPosition(transform.position);

            GameObject fakeBombInstance = Instantiate(_fakeBombPrefab,alignedPosition , Quaternion.identity);
            _elapsedTime = 0f;

            Destroy(fakeBombInstance,_cooldown / 2f);
        }
    }
}
