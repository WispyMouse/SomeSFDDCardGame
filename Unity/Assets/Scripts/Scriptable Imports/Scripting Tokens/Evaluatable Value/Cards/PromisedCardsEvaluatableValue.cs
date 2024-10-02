using System.Collections.Generic;
using SFDDCards.Evaluation.Actual;

namespace SFDDCards.ScriptingTokens.EvaluatableValues
{
    public class PromisedCardsEvaluatableValue : CardsEvaluatableValue
    {
        public CardsEvaluatableValue InnerValue
        {
            get
            {
                return this.innerValue;
            }
            set
            {
                this.innerValue = value;
            }
        }
        public CardsEvaluatableValue SampledPool;
        public string DescriptionText = "?";

        public CardsEvaluatableValue innerValue { get; set; }

        public override string DescribeEvaluation()
        {
            if (this.DescriptionText != "?")
            {
                return this.DescriptionText;
            }

            if (this.innerValue != null)
            {
                return this.InnerValue.DescribeEvaluation();
            }

            if (this.SampledPool != null)
            {
                return this.SampledPool.DescribeEvaluation();
            }

            return this.DescriptionText;
        }

        public override bool Equals(CardsEvaluatableValue other)
        {
            if (other == this)
            {
                return true;
            }

            if (this.InnerValue == null)
            {
                return false;
            }

            if (this.InnerValue.Equals(other))
            {
                return true;
            }

            if ((other is PromisedCardsEvaluatableValue promise))
            {
                return this.InnerValue.Equals(promise.InnerValue);
            }

            return false;
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