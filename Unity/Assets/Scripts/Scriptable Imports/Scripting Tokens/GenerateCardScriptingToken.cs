namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class GenerateCardScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken, ILaterZoneListenerScriptingToken
    {
        public override string ScriptingTokenIdentifier { get; } = "GENERATECARD";
        public string Id { get; set; }
        public IEvaluatableValue<int> NumberOfCards { get; set; } = null;

        public string LaterRealizedDestinationZone { get; set; }

        public bool ShouldSilenceSpeaker => true;

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

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return this.DescribeOperationAsEffect();
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            return this.DescribeOperationAsEffect();
        }

        private string DescribeOperationAsEffect()
        {
            string creationText = "";

            if (this.NumberOfCards is ConstantEvaluatableValue<int> constant && constant.ConstantValue == 1)
            {
                creationText = $"Create {CardDatabase.GetModel(this.Id).Name}";
            }
            else
            {
                creationText = $"Create {this.NumberOfCards.DescribeEvaluation()} {CardDatabase.GetModel(this.Id).Name}";
            }

            if (!string.IsNullOrEmpty(LaterRealizedDestinationZone))
            {
                switch (this.LaterRealizedDestinationZone.ToLower())
                {
                    case "hand":
                        creationText += " in hand";
                        break;
                    case "discard":
                        creationText += " in discard";
                        break;
                    case "exile":
                        creationText += " in exile";
                        break;
                }
            }

            return creationText;
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