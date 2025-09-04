using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using Game.Client.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Client
{
    public enum PlayerInputButtons
    {
        SprintButton,
        PlaceBombButton
    }

    public class PlayerInput : NetworkBehaviour, INetworkRunnerCallbacks
    {
        private InputSystemActions _inputSystemActions;
        private Vector2 _move;

        private void Awake() => _inputSystemActions = new InputSystemActions();

        public override void Spawned()
        {
            if (!HasInputAuthority) return;

            SetupInput();
            Runner?.AddCallbacks(this);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (!HasInputAuthority) return;

            CleanupInput();
            Runner?.RemoveCallbacks(this);
        }

        private void SetupInput()
        {
            _inputSystemActions.Player.Move.performed += OnMovePerformed;
            _inputSystemActions.Player.Move.canceled += OnMoveCancelled;
            _inputSystemActions.Enable();
        }

        private void CleanupInput()
        {
            _inputSystemActions.Disable();
            _inputSystemActions.Player.Move.performed -= OnMovePerformed;
            _inputSystemActions.Player.Move.canceled -= OnMoveCancelled;
        }

        private void OnMovePerformed(InputAction.CallbackContext ctx) => _move = ctx.ReadValue<Vector2>();
        private void OnMoveCancelled(InputAction.CallbackContext ctx) => _move = Vector2.zero;

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
            if (!HasInputAuthority) return;

            var inputState = new PlayerInputState { move = _move };
            inputState.buttons.Set(PlayerInputButtons.SprintButton, _inputSystemActions.Player.Sprint.IsPressed());
            inputState.buttons.Set(PlayerInputButtons.PlaceBombButton, _inputSystemActions.Player.PlaceBomb.IsPressed());
            input.Set(inputState);
        }

        private void OnDestroy()
        {
            // Ensure cleanup happens even if Despawned wasn't called
            if (_inputSystemActions != null)
            {
                _inputSystemActions.Disable();
                _inputSystemActions?.Dispose();
            }
        }

        #region Empty INetworkRunnerCallbacks functions
        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) {}
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {}
        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {}
        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {}
        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) {}
        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) {}
        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {}
        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) {}
        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) {}
        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) {}
        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) {}
        public void OnConnectedToServer(NetworkRunner runner) {}
        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {}
        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) {}
        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {}
        public void OnSceneLoadDone(NetworkRunner runner) {}
        public void OnSceneLoadStart(NetworkRunner runner) {}
        #endregion
    }

    public class PlayerInputSender : MonoBehaviour, INetworkInput {}

    public struct PlayerInputState : INetworkInput
    {
        public Vector2 move;
        public NetworkButtons buttons;
    }
}
