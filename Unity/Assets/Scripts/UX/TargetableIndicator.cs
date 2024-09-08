namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class TargetableIndicator : MonoBehaviour
    {
        ICombatantTarget Target { get; set; }
        Action<ICombatantTarget> OnClickAction { get; set; }

        public void SetFromTarget(ICombatantTarget target, Action<ICombatantTarget> onClickAction)
        {
            this.Target = target;
            this.OnClickAction = onClickAction;
        }

        public void OnMouseUpAsButton()
        {
            this.OnClickAction.Invoke(this.Target);
        }
    }
}