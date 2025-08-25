using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BombVfx : VfxController
{
    [SerializeField] private Transform target;

    [Header("Particles")]
    [SerializeField] private ParticleSystem fuseSparks; // loops while wiggling
    [SerializeField] private ParticleSystem prePopPuff; // burst before pop
    [SerializeField] private ParticleSystem explosionPrefab;  // final boom

    [Header("Wiggle")]
    [SerializeField] private float wiggleAngle = 10f;
    [SerializeField] private float wiggleStep = 0.2f;

    [Header("Shrink")]
    [SerializeField] private float shrinkScale = 0.8f;
    [SerializeField] private float shrinkTime = 0.1f;

    [Header("Pop")]
    [SerializeField] private float popScale = 1.5f;
    [SerializeField] private float popTime = 0.2f;

    [Header("Explode Positions")]
    [SerializeField] private List<Transform> _explodePositions;

    protected override void OnEnable()
    {
        base.OnEnable();
        Play(0.5f - 0.1f);
    }

    /// <summary>
    /// Generates explosion effects at the given world positions.
    /// (Tip: if you use a Blocks as Terrain (And We Are!), pass Block center point)
    /// /// </summary>
    public void PlayExplosionEffects(IReadOnlyList<Vector3> positions)
    {
        if (explosionPrefab == null || positions == null || positions.Count == 0)
            return;

        //Transform parent =  // This will be the parent of the instantiated particle systems

        for (int i = 0; i < positions.Count; i++)
        {
            ParticleSystem ps = Instantiate(explosionPrefab, positions[i], Quaternion.identity);
            ps.Play(true);

            // If prefab doesn't already destroy itself, schedule cleanup:
            ParticleSystem.MainModule main = ps.main;
            if (main.stopAction == ParticleSystemStopAction.None)
            {
                // duration + max start lifetime is a decent upper bound
                float lifetime =
                    main.duration +
                    (main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                        ? main.startLifetime.constantMax
                        : main.startLifetime.constant);

                Destroy(ps.gameObject, lifetime + 0.1f);
            }
        }
    }
    protected override Sequence BuildSequence(float duration)
    {
        if (!target)
        {
            Debug.LogError("BombVfx: Target transform is not assigned!"); // Early exit if no target
            return null;
        }

        Sequence sequence = DOTween.Sequence();

        float reserved = shrinkTime + popTime;
        float wiggleDuration = Mathf.Max(0f, duration - reserved);
        int loops = wiggleStep <= 0f ? 0 : Mathf.Max(0, Mathf.RoundToInt(wiggleDuration / wiggleStep));

        // Start fuse
        sequence.AppendCallback(StartFuseEffect);

        // WIGGLE
        if (loops > 0)
        {
            // Randomly choose between left-right (Z) or top-down (X)
            Vector3 axis;
            if (Random.value < 0.5f)
                axis = new Vector3(0, 0, wiggleAngle); // left–right tilt
            else
                axis = new Vector3(wiggleAngle, 0, 0); // top–down tilt

            sequence.Append(
                target.DORotate(axis, wiggleStep * 0.5f)
                      .SetEase(Ease.InOutSine)
                      .SetLoops(loops, LoopType.Yoyo)
            );
        }

        sequence.AppendCallback(() =>
        {
            StopFuseEffect();
        });


        // SHRINK
        sequence.Append(target.DOScale(Vector3.one * shrinkScale, shrinkTime).SetEase(Ease.InQuad));

        // POP
        sequence.Append(target.DOScale(Vector3.one * popScale, popTime).SetEase(Ease.OutBack));

        sequence.Append(target.DOScale(Vector3.one * 0, shrinkTime).SetEase(Ease.InQuad));

        // Stop fuse + puff
        sequence.AppendCallback(() =>
        {
            //StopFuseEffect();
            PlayEffectOnce(prePopPuff);
        });

        return sequence;
    }

    #region Particle helpers
    protected override void OnComplete()
    {
        List<Vector3> explodePositions = _explodePositions.Select(t => t.position).ToList();
        explodePositions.Add(this.transform.position); // Adding the bomb's position to the explosion positions - Total of 5 positions

        PlayExplosionEffects(explodePositions); // Temporary, for testing Until gifure out how to pass positions of Blocks\Tiles
        base.OnComplete();
    }
    private void StartFuseEffect()
    {
        if (fuseSparks != null)
        {
            var main = fuseSparks.main;
            if (!main.loop) main.loop = true;
            fuseSparks.Play(true);
        }
    }

    private void StopFuseEffect() // Stop the fuse sparks object
    {
        if (fuseSparks != null)
            fuseSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void PlayEffectOnce(ParticleSystem particleSystem)
    {
        if (particleSystem == null) return;

        ParticleSystem.MainModule main = particleSystem.main;

        if (main.loop) // make sure it doesn't loop 
            main.loop = false;

        particleSystem.Play(true);
    }

    #endregion
}
