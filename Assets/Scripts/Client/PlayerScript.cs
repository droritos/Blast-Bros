using Fusion;
using Game.Data;
using Game.Server;
using NUnit.Framework;
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
        [SerializeField] private Rigidbody playerRigidbody;

        [Header("Bomb Related Stuff")]
        [SerializeField] private BombInputFeedback _bombInputFeedback;
        private event UnityAction OnTryPlaceBomb;

        private bool _isSprinting;
        private NetworkButtons _prevButtons;

        private const float SpeedMultiplier = 2.5f;

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
                /*
            if (!_bombInputFeedback)
            {
                _bombInputFeedback = GetComponentInChildren<BombInputFeedback>();
            }
                */
            if (!playerRigidbody)
            {
                playerRigidbody = GetComponent<Rigidbody>();
            }

            var networkAnimator = GetComponentInChildren<NetworkMecanimAnimator>();
            networkAnimator.Animator = animator;
        }
#endif

        public override void FixedUpdateNetwork()
        {
            if((!HasInputAuthority && !HasStateAuthority) || !GetInput(out PlayerInputState input))
            {
                return;
            }

            HandleButtonInput(input);
            HandleMovement(input);

            _prevButtons = input.buttons;
        }

        private void HandleMovement(PlayerInputState input)
        {
            var inputMove = new Vector3(input.move.x, 0f, input.move.y);

            if (inputMove.sqrMagnitude < 0.01f)
            {
                return;
            }

            var deltaMove = inputMove * (speed * Runner.DeltaTime) * (_isSprinting ? SpeedMultiplier : 1);

            // Use the rigidbody's actual collider shape for collision detection
            if (playerRigidbody.SweepTest(deltaMove.normalized, out _, deltaMove.magnitude, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Debug.Log(deltaMove);

            playerRigidbody.MoveRotation(Quaternion.LookRotation(inputMove));
            playerRigidbody.MovePosition(playerRigidbody.position + deltaMove);

            float speedPercent = inputMove.magnitude * (_isSprinting ? SpeedMultiplier : 1);
            animator.SetFloat(AnimatorParams.Speed, speedPercent);
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
