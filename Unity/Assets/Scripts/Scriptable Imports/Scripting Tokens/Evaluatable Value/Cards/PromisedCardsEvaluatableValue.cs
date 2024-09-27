using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class PromisedCardsEvaluatableValue : CardsEvaluatableValue
    {
        public CardsEvaluatableValue InnerValue;
        public CardsEvaluatableValue SampledPool;
        public string DescriptionText = "?";

        public override string DescribeEvaluation()
        {
            if (this.InnerValue == null)
            {
                return this.DescriptionText;
            }

            return this.InnerValue.DescribeEvaluation();
        }

        public override string GetScriptingTokenText()
        {
            if (this.InnerValue == null)
            {
                return this.DescriptionText;
            }

            return this.InnerValue.GetScriptingTokenText();
        }

        public override int RepresentingNumberOfCards(DeltaEntry toApplyTo)
        {
            if (this.InnerValue == null)
            {
                return 0;
            }
            else
            {
                return this.InnerValue.RepresentingNumberOfCards(toApplyTo);
            }
        }

        public override bool TryEvaluateValue(CampaignContext campaignContext, TokenEvaluatorBuilder currentBuilder, out List<Card> evaluatedValue)
        {
            evaluatedValue = null;

            if (this.InnerValue == null && this.SampledPool != null)
            {
                if (this.SampledPool.TryEvaluateValue(campaignContext, currentBuilder, out evaluatedValue))
                {
                    return true;
                }
            }

            if (this.InnerValue == null || !this.InnerValue.TryEvaluateValue(campaignContext, currentBuilder, out evaluatedValue))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate promised value. Was it provided before the evaluation was requested?", GlobalUpdateUX.LogType.RuntimeError);
                return false;
            }

            return true;
        }
    }
}