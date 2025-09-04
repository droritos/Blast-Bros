using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Client.UI
{
    public class GameLeaderboardsPlayerSpot : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameTMP;
        [SerializeField] private Image playerSprite;

        public void UpdateDetails(string name, Sprite sprite)
        {
            nameTMP.SetText(name);
            playerSprite.sprite = sprite;
        }
    }
}
