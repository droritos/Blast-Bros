using System;
using Fusion;
using UnityEngine;

namespace Game
{
    public class PlayerData : NetworkBehaviour
    {
        [Networked] [OnChangedRender(nameof(NotifyPlayerNameChanged))] public NetworkString<_64> PlayerName { get; private set; }
        [Networked] [OnChangedRender(nameof(NotifyCharacterIndexChanged))] public int CharacterIndex { get; private set; } = -1;
        [Networked] public NetworkObject PhysicalPlayerObject { get; set; }

        private void NotifyPlayerNameChanged() => OnPlayerNameChanged?.Invoke(PlayerName.Value);
        private void NotifyCharacterIndexChanged() => OnCharacterIndexChanged?.Invoke(CharacterIndex);
        public event Action<string> OnPlayerNameChanged;
        public event Action<int> OnCharacterIndexChanged;

        public void UpdatePhysicalPlayerObject(NetworkObject newPhysicalPlayerObject) => PhysicalPlayerObject = newPhysicalPlayerObject;

        public void UpdatePlayerName(string newPlayerName)
        {
            if (Object.HasStateAuthority && !string.IsNullOrEmpty(newPlayerName))
            {
                PlayerName = newPlayerName;
            }
        }

        public void UpdateCharacterIndex(int newCharacterIndex)
        {
            if (Object.HasStateAuthority && newCharacterIndex >= 0)
            {
                CharacterIndex = newCharacterIndex;
            }
        }
    }
}
