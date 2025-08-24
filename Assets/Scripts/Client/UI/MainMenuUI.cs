using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button hostGameButton;
        [SerializeField] private Button quitButton;

        private void OnEnable()
        {
            startGameButton.onClick.AddListener(StartGame);
            hostGameButton.onClick.AddListener(HostGame);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable()
        {
            startGameButton.onClick.RemoveListener(StartGame);
            hostGameButton.onClick.RemoveListener(HostGame);
            quitButton.onClick.RemoveListener(QuitGame);
        }

        private void Start() => EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);

        private void StartGame() {}

        private void HostGame() {}

        private void QuitGame()
        {
            Debug.Log("Application is quitting...");
            #if UNITY_EDITOR
            if (Application.isEditor && Application.isPlaying)
            {
                UnityEditor.EditorApplication.ExitPlaymode();
                return;
            }
            #endif
            Application.Quit();
        }
    }
}
