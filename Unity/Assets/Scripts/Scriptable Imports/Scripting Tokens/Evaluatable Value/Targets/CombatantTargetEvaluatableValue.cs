using System.Collections.Generic;
using System.Text;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public abstract class CombatantTargetEvaluatableValue : IEvaluatableValue<ICombatantTarget>
    {
        public abstract string DescribeEvaluation();

        public abstract bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue);
    }
}