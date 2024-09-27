namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class ChooseCardScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> NumberOfCards { get; private set; }
        public PromisedCardsEvaluatableValue PromisedCards { get; private set; } = new PromisedCardsEvaluatableValue();

        public override string ScriptingTokenIdentifier { get; } = "CHOOSECARDS";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ChoiceToMake = new PlayerChooseFromCardBrowser(this.PromisedCards, this.NumberOfCards);

            if (tokenBuilder.RelevantCards is HandCardsEvaluatableValue)
            {
                this.PromisedCards.DescriptionText = $"{this.NumberOfCards.DescribeEvaluation()} {EffectDescriberDatabase.ExtractSingularOrPlural(this.NumberOfCards, "card")}";
            }
            else
            {
                this.PromisedCards.DescriptionText = $"{this.NumberOfCards.DescribeEvaluation()} {EffectDescriberDatabase.ExtractSingularOrPlural(this.NumberOfCards, "card")} from {tokenBuilder.RelevantCards.DescribeEvaluation()}";
            }

            this.PromisedCards.SampledPool = tokenBuilder.RelevantCards;
            tokenBuilder.RelevantCards = this.PromisedCards;
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

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }

        public override bool RequiresTarget()
        {
            return false;
        }
    }
}