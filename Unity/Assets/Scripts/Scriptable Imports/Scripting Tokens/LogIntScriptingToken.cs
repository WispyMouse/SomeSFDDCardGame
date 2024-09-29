namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class LogIntScriptingToken : BaseScriptingToken
    {
        public IEvaluatableValue<int> ValueToLog { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "LOGINT";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Intensity = this.ValueToLog;
            tokenBuilder.ActionsToExecute.Add((DeltaEntry currentDelta) =>
            {
                if (this.ValueToLog.TryEvaluateValue(currentDelta?.MadeFromBuilder?.Campaign, currentDelta?.MadeFromBuilder, out int evaluatedValue))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke(evaluatedValue.ToString(), GlobalUpdateUX.LogType.Info);

                    if (currentDelta?.MadeFromBuilder != null)
                    {
                        currentDelta.MadeFromBuilder.Intensity = evaluatedValue;
                    }
                }
            });
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