using Fusion;
using UnityEngine;

namespace Game
{
    public class PlayerData : NetworkBehaviour
    {
        [Networked] public NetworkString<_64> PlayerName { get; private set; }
        [Networked] public int CharacterIndex { get; private set; } = -1;
        [Networked] public NetworkObject PhysicalPlayerObject { get; set; }

        // Client requests updates, host validates and applies them
        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_UpdatePlayerName(string newPlayerName)
        {
            // Host validates and applies the change
            if (Object.HasStateAuthority && !string.IsNullOrEmpty(newPlayerName))
            {
                PlayerName = newPlayerName;
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPC_UpdateCharacterIndex(int newCharacterIndex)
        {
            // Host validates and applies the change
            if (Object.HasStateAuthority && newCharacterIndex >= 0)
            {
                CharacterIndex = newCharacterIndex;
            }
        }

        // Local-only method for physical objects (not networked)
        public void UpdatePhysicalPlayerObject(NetworkObject newPhysicalPlayerObject) => PhysicalPlayerObject = newPhysicalPlayerObject;

        // Convenience methods for clients to request updates
        public void UpdatePlayerName(string newPlayerName)
        {
            if (Object.HasInputAuthority)
            {
                RPC_UpdatePlayerName(newPlayerName);
            }
        }

        public void UpdateCharacterIndex(int newCharacterIndex)
        {
            if (Object.HasInputAuthority)
            {
                RPC_UpdateCharacterIndex(newCharacterIndex);
            }
        }
    }
}
