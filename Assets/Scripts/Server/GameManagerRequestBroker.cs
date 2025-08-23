using Fusion;
using System;
using UnityEngine;

namespace Game.Server
{
    public static class GameManagerRequestBroker
    {
        public static event Action<PlayerRef,Vector3> OnRequestBomb;
        public static void RequestBomb(PlayerRef playerRef,Vector3 position) => OnRequestBomb?.Invoke(playerRef,position);
    }
}
