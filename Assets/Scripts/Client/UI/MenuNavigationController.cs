using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace Game.Client.UI
{
    public class MenuNavigationController : MonoBehaviour
    {
        [Header("First Button to Select")]
        [SerializeField] GameObject firstSelected;

        [Header("Buttons")]
        [SerializeField] GameObject clientSelectButton;
        [SerializeField] GameObject serverSelectButton;

        private GameObject _currentSelected;

        void Start()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
            _currentSelected = firstSelected;
        }

        void Update()
        {
            // If nothing is selected, reselect the last known good selection
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(_currentSelected);
            }
            else
            {
                _currentSelected = EventSystem.current.currentSelectedGameObject;
            }
        }

        public void ChangeCurrentToClient()
        {
            _currentSelected = clientSelectButton;
            EventSystem.current.SetSelectedGameObject(_currentSelected);
        }
        public void ChangeCurrentToServer()
        {
            _currentSelected = serverSelectButton;
            EventSystem.current.SetSelectedGameObject(_currentSelected);
        }
    }
}
