namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class GenerateCardScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public override string ScriptingTokenIdentifier { get; } = "GENERATECARD";
        public string Id { get; set; }
        public IEvaluatableValue<int> NumberOfCards { get; set; } = null;

        public PromisedCardsEvaluatableValue ReferencedPromise = new PromisedCardsEvaluatableValue();

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RelevantCards = ReferencedPromise;
            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> remainingArguments))
            {
                output = new ConstantEvaluatableValue<int>(1);
            }

            if (remainingArguments.Count != 1)
            {
                return false;
            }

            scriptingToken = new GenerateCardScriptingToken()
            {
                NumberOfCards = output,
                Id = arguments[0].ToLower()
            };

            return true;
        }

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return $"Create {Id}";
        }

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            List<Card> generatedCards = new List<Card>();

            this.NumberOfCards.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out int value);
            for (int ii = 0; ii < value; ii++)
            {
                generatedCards.Add(CardDatabase.GetModel(this.Id));
            }

            this.ReferencedPromise.InnerValue = new SpecificCardsEvaluatableValue(generatedCards);
            stackedDeltas = null;
        }
    }
}