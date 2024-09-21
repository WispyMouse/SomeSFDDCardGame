namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class FoeTargetEvaluatableValue : CombatantTargetEvaluatableValue
    {
        public override string DescribeEvaluation()
        {
            return "Foe";
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out ICombatantTarget evaluatedValue)
        {
            if (TryGetFoe(campaignContext, currentBuilder, currentBuilder.User, out evaluatedValue))
            {
                return true;
            }

            evaluatedValue = null;
            return false;
        }

        private bool TryGetFoe(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, ICombatantTarget user, out ICombatantTarget foe)
        {
            // If the original target still exists, and is a foe of the user,
            // then use the original target
            if (currentBuilder.OriginalTarget != null
                && currentBuilder.OriginalTarget.Valid()
                && currentBuilder.OriginalTarget.IsFoeOf(user))
            {
                foe = currentBuilder.OriginalTarget;
                return true;
            }

            if (user is Enemy)
            {
                foe = campaignContext.CampaignPlayer;
                return true;
            }

            if (user is Player)
            {
                if (campaignContext.CurrentCombatContext.Enemies.Count > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, campaignContext.CurrentCombatContext.Enemies.Count);
                    foe = campaignContext.CurrentCombatContext.Enemies[randomIndex];
                    return true;
                }
            }

            foe = null;
            return false;
        }
    }
}