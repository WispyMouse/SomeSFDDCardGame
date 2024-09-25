namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.ImportModels.StatusEffectImport;

    public interface IStatusEffect
    {
        public EffectDescription DescribeStatusEffect();
    }
}