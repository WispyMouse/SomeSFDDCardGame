namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class SetCurrencyScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public IEvaluatableValue<int> Amount { get; private set; }
        public string CurrencyToMod { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "SETCURRENCY";

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            int amount;
            this.Amount.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out amount);

            applyingDuringEntry.FromCampaign.SetCurrency(CurrencyDatabase.GetModel(this.CurrencyToMod), amount);
            stackedDeltas = null;
        }

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return $"Sets {CurrencyDatabase.GetModel(this.CurrencyToMod).GetNameAndMaybeIcon()} to {this.Amount.DescribeEvaluation()}";
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            return $"Sets {CurrencyDatabase.GetModel(this.CurrencyToMod).GetNameAndMaybeIcon()} to {this.Amount.DescribeEvaluation()}";
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

            scriptingToken = new SetCurrencyScriptingToken()
            {
                Amount = output,
                CurrencyToMod = remainingArgument
            };

            return true;
        }
    }
}