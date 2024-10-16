namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.ImportModels.StatusEffectImport;

    public class WindowResponder
    {
        public string WindowId;
        public AttackTokenPile Effect;
        public int ApplicationPriority = 0;
    }
}