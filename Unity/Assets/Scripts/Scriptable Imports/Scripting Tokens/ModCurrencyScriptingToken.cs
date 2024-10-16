namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class ModCurrencyScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> Amount { get; private set; }
        public string CurrencyToMod { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "MODCURRENCY";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.CurrencyMod;
            tokenBuilder.Intensity = Amount;
            tokenBuilder.Currency = CurrencyDatabase.GetModel(CurrencyToMod);
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // We need to parse the strings for the stack amount and then the status effect
            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> remainingArguments))
            {
                return false;
            }

            // There should be one argument remaining, the status effect
            if (remainingArguments.Count != 1)
            {
                return false;
            }

            string remainingArgument = remainingArguments[0];

            scriptingToken = new ModCurrencyScriptingToken()
            {
                Amount = output,
                CurrencyToMod = remainingArgument
            };

            return true;
        }
    }
}