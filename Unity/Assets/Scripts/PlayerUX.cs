namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    public class PlayerUX : MonoBehaviour
    {
        public Player RepresentedPlayer { get; private set; }
        Action OnPlayerClick { get; set; }

        public void OnMouseDown()
        {
            this.OnPlayerClick.Invoke();
        }

        public void SetFromPlayer(Player toSet, Action onPlayerClick)
        {
            this.RepresentedPlayer = toSet;
            this.OnPlayerClick = onPlayerClick;
        }
    }
}