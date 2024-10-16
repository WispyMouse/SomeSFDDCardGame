using SFDDCards.Evaluation.Actual;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{

    public interface INumericEvaluatableValue : IEvaluatableValue<int>
    {
        bool TryEvaluateDecimalValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentEvaluator, out decimal evaluatedValue);
    }
}