using System;
using System.Collections.Generic;
using System.Text;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public abstract class CombatantTargetEvaluatableValue : IEvaluatableValue<ICombatantTarget>, IEquatable<CombatantTargetEvaluatableValue>
    {
        public abstract string DescribeEvaluation();

        public bool Equals(CombatantTargetEvaluatableValue other)
        {
            return this == other;
        }

        public abstract bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue);
    }
}