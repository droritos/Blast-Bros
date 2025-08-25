using System;
using System.Collections.Generic;
using Game.Server;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Game.Client;

namespace Game
{ 
    public class PlayerProfileUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventoriesManager _manager;

        [SerializeField] private TextMeshProUGUI _name; 
        [SerializeField] private TextMeshProUGUI _currentBombText;
        [SerializeField] private Image characterIcon;

        private void OnEnable()
        {
            //_localPlayer.OnBombReqeust += UpdateBomb;
            InizilizeProfile("Me", null);
        }

        public void InizilizeProfile(string name, Sprite icon)
        {
            _name.SetText(name);
            characterIcon.sprite = icon;
            UpdateBombText(1,1);
        }

        private void UpdateBombText(string newAmount)
        {
            _currentBombText.SetText(newAmount);
        }
        private void UpdateBombText(int currentBombs, int totalBombs)
        {
            _currentBombText.SetText(currentBombs.ToString() + "/" + totalBombs.ToString());
        }

        private void UpdateBomb()
        {
            UpdateBombText(_manager.LocalPlayerCurrentBombCount, _manager.LocalPlayerMaxBombCount);
        }
    }
}
