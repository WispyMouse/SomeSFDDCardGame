using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class AllFoeTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "All Foes";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            if (TryGetAllFoes(campaignContext, currentBuilder, currentBuilder.User, out evaluatedValue))
            {
                return true;
            }

            evaluatedValue = null;
            return false;
        }

        private bool TryGetAllFoes(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, ICombatantTarget user, out ICombatantTarget foe)
        {
            if (user is Enemy)
            {
                foe = campaignContext.CampaignPlayer;
                return true;
            }

            if (user is Player)
            {
                if (campaignContext.CurrentCombatContext.Enemies.Count > 0)
                {
                    foe = new AllFoesTarget(new List<ICombatantTarget>(campaignContext.CurrentCombatContext.Enemies));
                    return true;
                }
            }

            foe = null;
            return false;
        }
    }
}