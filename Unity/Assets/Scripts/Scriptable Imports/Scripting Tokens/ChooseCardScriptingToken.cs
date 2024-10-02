namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class ChooseCardScriptingToken : BaseScriptingToken, ILaterZoneListenerScriptingToken, IRealizedOperationScriptingToken
    {
        public IEvaluatableValue<int> NumberOfCards { get; private set; }
        public PromisedCardsEvaluatableValue PromisedCards { get; private set; } = new PromisedCardsEvaluatableValue();

        public string LaterRealizedDestinationZone
        {
            get
            {
                return _laterRealizedDestinationZone;
            }
            set
            {
                _laterRealizedDestinationZone = value;
                this.UpdateDescriptionForPromise();
            }
        }
        private string _laterRealizedDestinationZone { get; set; }

        public override string ScriptingTokenIdentifier { get; } = "CHOOSECARDS";

        public bool ShouldSilenceSpeaker => false;

        private ConceptualTokenEvaluatorBuilder FromBuilder;

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ChoiceToMake = new PlayerChooseFromCardBrowser(this.PromisedCards, this.NumberOfCards);
            this.PromisedCards.SampledPool = tokenBuilder.RelevantCards;
            tokenBuilder.RelevantCards = this.PromisedCards;
            this.FromBuilder = tokenBuilder;
            tokenBuilder.RealizedOperationScriptingToken = this;

            this.UpdateDescriptionForPromise();
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> _))
            {
                return false;
            }

            scriptingToken = new ChooseCardScriptingToken()
            {
                NumberOfCards = output
            };

            return true;
        }

        void UpdateDescriptionForPromise()
        {
            // If this choice is leading to discarding a card from hand, we don't have to include the zone
            if (this.PromisedCards.DescriptionText != "?")
            {
                return;
            }

            if (!string.IsNullOrEmpty(LaterRealizedDestinationZone) && LaterRealizedDestinationZone == "discard" && FromBuilder.RelevantCards is HandCardsEvaluatableValue)
            {
                this.PromisedCards.DescriptionText = $"{this.NumberOfCards.DescribeEvaluation()} {EffectDescriberDatabase.ExtractSingularOrPlural(this.NumberOfCards, "card")}";
            }
            else
            {
                this.PromisedCards.DescriptionText = $"{this.NumberOfCards.DescribeEvaluation()} {EffectDescriberDatabase.ExtractSingularOrPlural(this.NumberOfCards, "card")} from {this.PromisedCards.DescribeEvaluation()}";
            }
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return string.Empty;
        }

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            stackedDeltas = null;
        }
    }
}