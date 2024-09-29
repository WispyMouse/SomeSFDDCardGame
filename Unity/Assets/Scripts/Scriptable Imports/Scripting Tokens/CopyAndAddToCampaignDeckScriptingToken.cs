namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class CopyAndAddToCampaignDeckScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public override string ScriptingTokenIdentifier { get; } = "COPYANDADDTOCAMPAIGNDECK";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = new CopyAndAddToCampaignDeckScriptingToken();
            return true;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return "Add the selected cards to your campaign deck";
        }

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            applyingDuringEntry.RelevantCards.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out List<Card> cards);
            foreach (Card card in new List<Card>(cards))
            {
                applyingDuringEntry.FromCampaign.AddCardToDeck(card);
            }
            stackedDeltas = null;
        }
    }
}