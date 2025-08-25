using System;
using System.Collections.Generic;
using Game.Server;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game
{
    public class CharacterSelectUI : MonoBehaviour
    {
        [SerializeField] private GameData gameData;
        [SerializeField] private CharacterSelectionManager manager;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private RectTransform gridRoot;
        [SerializeField] private Button readyButton;

        private readonly List<Button> _characterButtons = new();
        private int[] _lastOccupied = Array.Empty<int>();
        private int _selectedCharacterIdx;
        private bool _isPlayerReady;

        private void Awake()
        {
            for (int characterIdx = 0; characterIdx < gameData.characters.Length; characterIdx++)
            {
                InitializeCharacterButton(characterIdx);
            }

            readyButton.interactable = false;
        }

        private void Start() => EventSystem.current.SetSelectedGameObject(_characterButtons[0].gameObject);

        private void OnEnable()
        {
            manager.OnCharacterRequestResponse += ManagerOnOnCharacterRequestResponse;
            manager.OnUpdateCharacterMarkedStatus += ManagerOnOnUpdateCharacterMarkedStatus;
            readyButton.onClick.AddListener(OnPlayerReady);
        }

        private void OnDisable()
        {
            manager.OnCharacterRequestResponse -= ManagerOnOnCharacterRequestResponse;
            manager.OnUpdateCharacterMarkedStatus -= ManagerOnOnUpdateCharacterMarkedStatus;
            readyButton.onClick.RemoveListener(OnPlayerReady);
        }

        private void OnPlayerReady()
        {
            Debug.Log("Player is now ready...");
            _isPlayerReady = true;
            readyButton.interactable = false;
            manager.MarkReadyRPC();
            gameObject.SetActive(false);
        }

        private void InitializeCharacterButton(int characterIdx)
        {
            var data = gameData.characters[characterIdx];
            var buttonGameObject = Instantiate(buttonPrefab, gridRoot);

            var button = buttonGameObject.GetComponent<Button>();
            int currentCharacterIndex = characterIdx;
            button.onClick.AddListener(() => RequestCharacter(currentCharacterIndex));

            var btnImageGameObject = buttonGameObject.transform.GetChild(0).gameObject;
            var btnImage = btnImageGameObject.GetComponent<Image>();
            btnImage.sprite = data.characterIcon;

            _characterButtons.Add(button);
        }

        private void RequestCharacter(int characterIdx)
        {
            if (_isPlayerReady) return;
            manager.RequestCharacterRPC(characterIdx);
        }

        private void ManagerOnOnUpdateCharacterMarkedStatus(int[] occupiedCharacters)
        {
            if (_lastOccupied.AsSpan().SequenceEqual(occupiedCharacters.AsSpan()))
            {
                return;
            }

            var occupiedHashset = new HashSet<int>(occupiedCharacters);
            for (int i = 0; i < _characterButtons.Count; ++i)
            {
                var button = _characterButtons[i];
                if (!occupiedHashset.Contains(i))
                {
                    button.interactable = true;
                    continue;
                }

                button.interactable = false;
                if (i == _selectedCharacterIdx)
                {
                    Debug.Log("This is my character!");
                    // TODO: set appropriate sprite here?
                }
            }

            _lastOccupied = occupiedCharacters;
        }

        private void ManagerOnOnCharacterRequestResponse(bool result, int characterIdx)
        {
            if (!result)
            {
                Debug.Log($"{gameData.characters[characterIdx].name} is already taken, sawry...");
                return;
            }

            Debug.Log($"Managed to select character {gameData.characters[characterIdx].name}");

            _selectedCharacterIdx = characterIdx;
            readyButton.interactable = !_isPlayerReady;
        }
    }
}
