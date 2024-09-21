using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;

namespace SFDDCards.ScriptingTokens
{
    public class GainElementScriptingToken : BaseScriptingToken
    {
        public string Element { get; private set; }
        public IEvaluatableValue<int> Amount { get; private set; }

        public override string ScriptingTokenIdentifier => "GAINELEMENT";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ElementResourceChanges.Add(new ElementResourceChange() { Element = ElementDatabase.GetElement(this.Element), GainOrLoss = this.Amount });
        }

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> remainingArgs))
            {
                return false;
            }

            if (remainingArgs.Count != 1)
            {
                return false;
            }

            scriptingToken = new GainElementScriptingToken()
            {
                Element = remainingArgs[0],
                Amount = output
            };

            return true;
        }
    }
}