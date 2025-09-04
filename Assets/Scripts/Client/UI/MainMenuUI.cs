using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Client.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button startServerButton;
        [SerializeField] private Button quitButton;

        [SerializeField] private CanvasGroup clientStartGameCanvas;
        [SerializeField] private CanvasGroup hostStartGameCanvas;

        [SerializeField] private MenuNavigationController menuNavigationController;

        private void OnEnable()
        {
            startGameButton.onClick.AddListener(StartGame);
            startServerButton.onClick.AddListener(HostGameServer);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable()
        {
            startGameButton.onClick.RemoveListener(StartGame);
            startServerButton.onClick.RemoveListener(HostGameServer);
            quitButton.onClick.RemoveListener(QuitGame);
        }

        private void Start() => EventSystem.current.SetSelectedGameObject(startGameButton.gameObject);

        private void StartGame()
        {
            clientStartGameCanvas.interactable = true;
            clientStartGameCanvas.blocksRaycasts = true;
            clientStartGameCanvas.alpha = 1;

            menuNavigationController.ChangeCurrentToClient();
        }

        private void HostGameServer()
        {
            hostStartGameCanvas.interactable = true;
            hostStartGameCanvas.blocksRaycasts = true;
            hostStartGameCanvas.alpha = 1;

            menuNavigationController.ChangeCurrentToServer();
        }

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

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if(!menuNavigationController)
            {
                menuNavigationController = FindAnyObjectByType<MenuNavigationController>();
            }
        }
        #endif
    }
}
