using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class SetTargetScriptingToken : BaseScriptingToken
    {
        public override string ScriptingTokenIdentifier => "SETTARGET";

        public CombatantTargetEvaluatableValue Target;

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Target = this.Target;
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
                case "original":
                    scriptingToken = new SetTargetScriptingToken() { Target = new OriginalTargetEvaluatableValue() };
                    return true;
            }

            return false;
        }
    }
}