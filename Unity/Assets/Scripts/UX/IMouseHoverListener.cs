namespace SFDDCards.UX
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public interface IMouseHoverListener
    {
        public Transform GetTransform();
        public bool TryGetCard(out Card toShow);
        public bool TryGetStatusEffect(out IStatusEffect toShow);
        public bool ShouldShowBase { get; }
    }
}