namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class PlayerUX : MonoBehaviour, IAnimationPuppet
    {
        public Player RepresentedPlayer { get; private set; }
        public Transform OwnTransform => this.transform;
        public bool IsNotDestroyed => this != null && this?.gameObject != null;

        public void SetFromPlayer(Player toSet)
        {
            this.RepresentedPlayer = toSet;
            this.RepresentedPlayer.UXPositionalTransform = this.transform;
        }
    }
}