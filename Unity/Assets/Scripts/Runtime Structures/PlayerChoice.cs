namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public abstract class PlayerChoice
    {
        public bool ResultIsChosen { get; protected set; } = false;
        public abstract string DescribeChoice(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator);
        public abstract bool TryFinalizeWithoutPlayerInput(DeltaEntry toApplyTo);
    }

    public abstract class PlayerChoice<T> : PlayerChoice
    {
        public T ChosenResult { get; private set; }
        public virtual void SetChoice(DeltaEntry toApplyTo, T result)
        {
            this.ChosenResult = result;
            this.ResultIsChosen = true;
        }
    }
}