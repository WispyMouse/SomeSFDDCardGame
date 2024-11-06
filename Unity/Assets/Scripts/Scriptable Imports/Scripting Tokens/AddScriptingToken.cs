namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class AddScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public override string ScriptingTokenIdentifier { get; } = "ADD";

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            if (applyingDuringEntry.RelevantCards != null && applyingDuringEntry.RelevantCards.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out List<Card> evaluatedCards))
            {
                // Just in case, set the evaluatedCards into their own list
                // such that the out above won't modify as we're removing the cards
                foreach (Card curCard in new List<Card>(evaluatedCards))
                {
                    applyingDuringEntry.FromCampaign.AddCardToDeck(curCard);
                }
            }

            stackedDeltas = null;
        }

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            if (delta?.MadeFromBuilder?.RelevantCards != null)
            {
                return $"Add {delta.MadeFromBuilder.RelevantCards.DescribeEvaluation()}";
            }

            return "Add selected";
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            if (builder?.RelevantCardsEvaluatable != null)
            {
                return $"Add {builder?.RelevantCardsEvaluatable.DescribeEvaluation()}";
            }

            return "Add selected";
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = new AddScriptingToken();
            return true;
        }
    }
}