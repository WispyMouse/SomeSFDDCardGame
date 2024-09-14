namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class LogIntScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> ValueToLog { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "LOGINT";

        public override void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            if (this.ValueToLog.TryEvaluateValue(null, tokenBuilder, out int evaluatedValue))
            {
                GlobalUpdateUX.LogTextEvent?.Invoke(evaluatedValue.ToString(), GlobalUpdateUX.LogType.Info);
            }
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (!TryGetIntegerEvaluatableFromStrings(arguments, out IEvaluatableValue<int> output, out List<string> _))
            {
                return false;
            }

            scriptingToken = new LogIntScriptingToken()
            {
                ValueToLog = output
            };

            return true;
        }
    }
}