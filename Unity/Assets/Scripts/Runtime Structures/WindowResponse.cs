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


    public struct WindowResponse
    {
        public WindowResponder FromResponder;
        public ReactionWindowContext FromContext;

        public AppliedStatusEffect FromStatusEffect;

        public WindowResponse(WindowResponder fromResponder, ReactionWindowContext fromContext, AppliedStatusEffect fromStatusEffect)
        {
            this.FromResponder = fromResponder;
            this.FromContext = fromContext;
            this.FromStatusEffect = fromStatusEffect;
        }
    }
}
