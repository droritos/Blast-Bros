using UnityEngine;
using UnityEngine.Events;

namespace Game.Client
{
    public class PlayerInventory
    {
        public event UnityAction OnBombCountChanged;
        public event UnityAction OnBombUseFailed;

        //public PlayerInventoriesManager InventoriesManager { get; private set; }

        public int TotalBombs { get; private set; } = 1; // Default to 1 bomb

        private int _currentBombCount; // _currentBombCount/TotalBombs - GUI
        public int CurrentBombCount
        {
            get => _currentBombCount;
            set
            {
                int newValue = Mathf.Clamp(value, 0, TotalBombs);
                if (_currentBombCount != newValue)
                {
                    _currentBombCount = newValue;
                    Debug.Log($"Current Bomb Count Updated: {_currentBombCount}");
                    OnBombCountChanged?.Invoke();
                }
            }
        }

        public PlayerInventory()
        {
            UpdateInventory();
            //InventoriesManager = inventoriesManager;
        }

        public void UpdateInventory()
        {
            CurrentBombCount = TotalBombs; // Initialize with max bomb count
        }

        public void AddBombCount()
        {
            TotalBombs++;
            UpdateInventory();
        }
        public void RestoreBombCount()
        {
            CurrentBombCount++;
        }

        public bool TryUseBomb()
        {
            if (CurrentBombCount > 0)
            {
                CurrentBombCount--;
                return true; // Bomb used successfully
            }
            else
            {
                OnBombUseFailed?.Invoke(); // Trigger UI or sound feedback
                return false; // No bombs available to use
            }
        }

        #region << Called From Server >>
        public void SetCurrentBombCount(int value)
        {
            CurrentBombCount = value;
        }

        public void NotifyUseFailed() 
        {
            OnBombUseFailed?.Invoke();
        }
        #endregion
    }
}
