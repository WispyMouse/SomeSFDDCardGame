namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class CardBrowserSelectorScriptingToken : BaseScriptingToken
    {
        public override string ScriptingTokenIdentifier => "CARDBROWSER";
        public CardsEvaluatableValue Cards { get; private set; }

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RelevantCards = this.Cards;
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // There can either be an evaluated number, or no numbers, when showing the card browser
            IEvaluatableValue<int> number;
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out number, out List<string> remainingArguments))
            {
                number = null;
            }

            // Besides the number, there should be exactly one argument specifying the zone
            if (remainingArguments.Count != 1)
            {
                return false;
            }

            scriptingToken = new CardBrowserSelectorScriptingToken()
            {
                Cards = CardsEvaluatableValue.GetEvaluatable(remainingArguments[0], number)
            };

            return true;
        }
    }
}