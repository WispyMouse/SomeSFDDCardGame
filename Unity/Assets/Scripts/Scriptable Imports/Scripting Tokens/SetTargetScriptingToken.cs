using SFDDCards.Evaluation.Actual;
using SFDDCards.Evaluation.Conceptual;
using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class SetTargetScriptingToken : BaseScriptingToken
    {
        public override string ScriptingTokenIdentifier => "SETTARGET";

        public CombatantTargetEvaluatableValue Target;

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Target = this.Target;

            if (tokenBuilder.OriginalTarget == null)
            {
                tokenBuilder.OriginalTarget = this.Target;
            }
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            // The only expected argument is the targeting value
            if (arguments.Count != 1)
            {
                return false;
            }

            string firstArgument = arguments[0].ToLower();

            switch (firstArgument)
            {
                case "self":
                    scriptingToken = new SetTargetScriptingToken() { Target = new SelfTargetEvaluatableValue() };
                    return true;
                case "foe":
                    scriptingToken = new SetTargetScriptingToken() { Target = new FoeTargetEvaluatableValue() };
                    return true;
                case "allfoes":
                    scriptingToken = new SetTargetScriptingToken() { Target = new AllFoeTargetEvaluatableValue() };
                    return true;
                case "original":
                    scriptingToken = new SetTargetScriptingToken() { Target = new OriginalTargetEvaluatableValue() };
                    return true;
            }

            return false;
        }
    }
}