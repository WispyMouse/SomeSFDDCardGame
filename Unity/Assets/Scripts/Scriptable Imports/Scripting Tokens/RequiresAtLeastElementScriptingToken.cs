using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;

namespace SFDDCards.ScriptingTokens
{
    public class RequiresAtLeastElementScriptingToken : BaseScriptingToken
    {
        public string Element { get; private set; }
        public IEvaluatableValue<int> Amount { get; private set; }

        public override string ScriptingTokenIdentifier => "requiresatleastelement";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            Element thisElement = ElementDatabase.GetElement(this.Element);

            if (tokenBuilder.ElementRequirements.ContainsKey(thisElement))
            {
                tokenBuilder.ElementRequirements[thisElement] = this.Amount;
            }
            else
            {
                tokenBuilder.ElementRequirements.Add(thisElement, this.Amount);
            }
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

            scriptingToken = new RequiresAtLeastElementScriptingToken()
            {
                Element = remainingArgs[0],
                Amount = output
            };

            return true;
        }
    }
}