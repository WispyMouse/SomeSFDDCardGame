namespace SFDDCards.UX
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class TargetableIndicator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        ICombatantTarget Target { get; set; }
        Action<ICombatantTarget> OnClickAction { get; set; } = null;
        Action<ICombatantTarget> OnHoverAction { get; set; } = null;
        Action<ICombatantTarget> OnHoverEndAction { get; set; } = null;

        public void SetFromTarget(ICombatantTarget target, Action<ICombatantTarget> onClickAction, Action<ICombatantTarget> onHoverStart, Action<ICombatantTarget> onHoverEnd)
        {
            this.Target = target;
            this.OnClickAction = onClickAction;
            this.OnHoverAction = onHoverStart;
            this.OnHoverEndAction = onHoverEnd;
        }

        public void OnMouseUpAsButton()
        {
            this.OnClickAction.Invoke(this.Target);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.OnHoverAction?.Invoke(this.Target);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            this.OnHoverEndAction?.Invoke(this.Target);
        }
    }
}