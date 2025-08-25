using LitMotion;
using LitMotion.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.VFX
{
    public class BombVFX : MonoBehaviour
    {
        [SerializeField] private ExplosionEffectSettings effectSettings;
        [SerializeField] private List<Transform> explodePositions;
        [SerializeField] ParticleSystem fuseSparks, prePopPuff, explosionPrefab;

        private MotionHandle _sequenceHandle;

        public void StopSequence()
        {
            if(_sequenceHandle.IsActive())
            {
                _sequenceHandle.Cancel();
            }
        }

        public void Start() => Play();
        public void OnDestroy() => StopSequence();

        private void Play()
        {
            var sequence = LSequence.Create();
            PlayWithLoop(fuseSparks);

            // Wiggle
            float wiggleDuration = effectSettings.duration - effectSettings.shrinkTime - effectSettings.popTime;
            if (wiggleDuration > 0 && effectSettings.wiggleStep > 0)
            {
                Vector3 wiggle = (UnityEngine.Random.value < 0.5f ? Vector3.forward : Vector3.right) * effectSettings.wiggleAngle;
                int loops = Mathf.RoundToInt(wiggleDuration / effectSettings.wiggleStep);

                sequence.Append(LMotion.Create(Vector3.zero, wiggle, wiggleDuration / loops)
                    .WithEase(Ease.InOutSine)
                    .WithLoops(loops, LoopType.Yoyo)
                    .WithOnComplete(DoPrePop)
                    .BindToEulerAngles(transform));
            }

            // Shrink -> Pop -> Explode
            sequence.Append(LMotion.Create(Vector3.one, Vector3.one * effectSettings.shrinkScale, effectSettings.shrinkTime).WithEase(Ease.InQuad)
                    .BindToLocalScale(transform))
                .Append(LMotion.Create(Vector3.one * effectSettings.shrinkScale, Vector3.one * effectSettings.popScale, effectSettings.popTime)
                    .WithEase(Ease.OutBack).BindToLocalScale(transform))
                .Append(LMotion.Create(Vector3.one * effectSettings.popScale, Vector3.zero, effectSettings.shrinkTime).WithEase(Ease.InQuad)
                    .WithOnComplete(Explode).BindToLocalScale(transform));

            _sequenceHandle = sequence.Run();
        }

        private void DoPrePop()
        {
            fuseSparks?.Stop();
            prePopPuff?.Play();
        }

        private void PlayWithLoop(ParticleSystem ps)
        {
            var main = ps.main;
            main.loop = true;
            ps.Play();
        }

        private void Explode()
        {
            foreach (var pos in explodePositions.Select(t => t.position))
            {
                var explosion = Instantiate(explosionPrefab, pos, Quaternion.identity);
                explosion.Play();

                var main = explosion.main;
                if (main.stopAction == ParticleSystemStopAction.None)
                    Destroy(explosion.gameObject, main.duration + main.startLifetime.constant + 0.1f);
            }
        }
    }
}
