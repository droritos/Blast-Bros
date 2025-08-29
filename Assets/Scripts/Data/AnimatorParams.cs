using UnityEngine;

namespace Game.Data
{
    public static class AnimatorParams
    {
        public const string Speed = "Speed";
        public const string EmotePose = "EmotePose"; // 1 - First Emote | 2 - Second Emote
    }

    public static class AnimationTriggers
    {
        public static readonly int PlaceBomb = Animator.StringToHash("Place Bomb");
        public static readonly int Death = Animator.StringToHash("Death");
    }
}
