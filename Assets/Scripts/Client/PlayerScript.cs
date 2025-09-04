using Fusion;
using Game.Data;
using Game.Server;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Client
{
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerScript : NetworkBehaviour
    {
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private Animator animator;
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private float speed = 5f;

        [Header("Bomb Related Stuff")]
        [SerializeField] private BombInputFeedback _bombInputFeedback;
        private event UnityAction OnTryPlaceBomb;

        private bool _isSprinting;
        private NetworkButtons _prevButtons;

        private const float SpeedMultiplier = 1.5f;

        public override void Spawned()
        {
            base.Spawned();
            OnTryPlaceBomb += _bombInputFeedback.TryPlaceFakeBomb;
        }
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            base.Despawned(runner, hasState);
            OnTryPlaceBomb -= _bombInputFeedback.TryPlaceFakeBomb;
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
            }

            if (!playerInput)
            {
                playerInput = GetComponent<PlayerInput>();
            }

            if (!capsuleCollider)
            {
                capsuleCollider = GetComponent<CapsuleCollider>();
            }

            if (!_bombInputFeedback)
            {
                _bombInputFeedback = GetComponentInChildren<BombInputFeedback>();
            }

            var networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
            networkAnimator.Animator = animator;
        }
#endif

        public override void FixedUpdateNetwork()
        {
            if (!GetInput(out PlayerInputState input))
            {
                return;
            }

            HandleMovement(input);
            HandleButtonInput(input);

            _prevButtons = input.buttons;
        }

        private void HandleMovement(PlayerInputState input)
        {
            var actualMove = new Vector3(input.move.x, 0f, input.move.y);
            var deltaMove = actualMove * (speed * Runner.DeltaTime);

            if (actualMove != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(actualMove);
            }

            if (_isSprinting)
            {
                deltaMove *= SpeedMultiplier;
            }

            if (!Physics.SphereCast(transform.position, capsuleCollider.radius, actualMove.normalized, out RaycastHit hit, deltaMove.magnitude, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore))
            {
                transform.position += deltaMove;

                float speedPercent = actualMove.magnitude;
                if (_isSprinting)
                    speedPercent *= SpeedMultiplier;

                animator.SetFloat(AnimatorParams.Speed, speedPercent);
            }
        }

        private void HandleButtonInput(PlayerInputState input)
        {
            if (!Object.HasInputAuthority)
            {
                return;
            }

            if (input.buttons.WasPressed(_prevButtons, PlayerInputButtons.PlaceBombButton))
            {
                GameManager.instance.RequestBombAtLocationRPC(transform.position);

                animator.SetTrigger(AnimationTriggers.PlaceBomb); // Change Animation!

                OnTryPlaceBomb?.Invoke();

            }
            if (input.buttons.WasPressed(_prevButtons, PlayerInputButtons.SprintButton))
            {
                _isSprinting = true;
            }
            else if (input.buttons.WasReleased(_prevButtons, PlayerInputButtons.SprintButton))
            {
                _isSprinting = false;
            }
        }
    }
}
