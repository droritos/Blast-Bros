using UnityEngine;

namespace Game.Server
{
    public static class CanvasGroupExtensions
    {
        public static void HideCanvasGroup(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        public static void ShowCanvasGroup(this CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }
}
