namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class ChooseRandomlyScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public IEvaluatableValue<int> NumberOfCards { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "CHOOSERANDOMLY";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            if (tokenBuilder.RelevantCards == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke("Attempted to apply an action that requires relevant cards selected without any.", GlobalUpdateUX.LogType.RuntimeError);
            }

            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> _))
            {
                output = new ConstantNumericEvaluatableValue(1);
            }

            scriptingToken = new ChooseRandomlyScriptingToken()
            {
                NumberOfCards = output
            };

            return true;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return this.DescribeOperationAsEffect();
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            return this.DescribeOperationAsEffect();
        }

        public string DescribeOperationAsEffect()
        {
            return $"Randomly choose {this.NumberOfCards.DescribeEvaluation()}";
        }

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            stackedDeltas = null;

            if (applyingDuringEntry.RelevantCards == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Relevant cards list is null. Cannot move cards.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            if (!applyingDuringEntry.RelevantCards.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out List<Card> cards))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse relevant cards.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            if (!this.NumberOfCards.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out int numberOfCards))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse number of cards.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            List<Card> randomizedList = cards.ShuffleList();
            List<Card> results = new List<Card>();

            for (int ii = 0; ii < numberOfCards && ii < randomizedList.Count; ii++)
            {
                results.Add(randomizedList[ii]);
            }

            applyingDuringEntry.RelevantCards = new SpecificCardsEvaluatableValue(results);
        }
    }
}