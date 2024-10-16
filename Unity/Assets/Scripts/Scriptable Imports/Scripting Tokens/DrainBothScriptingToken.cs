namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class DrainBothScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public string ReduceArgumentOne;
        public string ReduceArgumentTwo;

        public override string ScriptingTokenIdentifier { get; } = "DRAINBOTH";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (arguments.Count != 2)
            {
                return false;
            }

            // Always sort Intensity before the other, if that's the case
            string argumentOne = arguments[0], argumentTwo = arguments[1];
            if (arguments[1].ToLower() == "intensity")
            {
                argumentOne = arguments[1];
                argumentTwo = arguments[0];
            }

            scriptingToken = new DrainBothScriptingToken()
            {
                ReduceArgumentOne = argumentOne,
                ReduceArgumentTwo = argumentTwo
            };

            return true;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return this.DescribeOperationAsEffect(reactionWindowId);
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            return this.DescribeOperationAsEffect(builder.BasedOnConcept?.CreatedFromContext?.TimingWindowId);
        }

        public string DescribeOperationAsEffect(string reactionWindowId)
        {
            if (this.ReduceArgumentOne.ToLower() == "intensity" && reactionWindowId == KnownReactionWindows.DamageIncoming)
            {
                return $"Damage first subtracts from {this.ReduceArgumentTwo} before subtracting from health";
            }

            return $"Reduce {this.ReduceArgumentOne} by {this.ReduceArgumentTwo}.";
        }

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            if (!context.HasValue)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Attempted to apply a reaction realized token without a context.", GlobalUpdateUX.LogType.RuntimeError);
            }

            IEvaluatableValue<int> argumentOneValue = context.Value.ResultingDelta.GetArgumentValue(this.ReduceArgumentOne);
            IEvaluatableValue<int> argumentTwoValue = context.Value.ResultingDelta.GetArgumentValue(this.ReduceArgumentTwo);

            if (!argumentOneValue.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out int argumentOneEvaluatedValue) || !argumentTwoValue.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out int argumentTwoEvaluatedValue))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Could not evaluate both arguments.", GlobalUpdateUX.LogType.RuntimeError);
                stackedDeltas = null;
                return;
            }

            int argumentOneFinalValue = argumentOneEvaluatedValue > argumentTwoEvaluatedValue ? argumentOneEvaluatedValue - argumentTwoEvaluatedValue : 0;
            int argumentTwoFinalValue = argumentTwoEvaluatedValue > argumentOneEvaluatedValue ? argumentTwoEvaluatedValue - argumentOneEvaluatedValue : 0;

            DeltaEntry pushDeltaOne = context.Value.ResultingDelta.SetArgumentValue(ReduceArgumentOne, argumentOneFinalValue);
            DeltaEntry pushDeltaTwo = context.Value.ResultingDelta.SetArgumentValue(ReduceArgumentTwo, argumentTwoFinalValue);

            stackedDeltas = new List<DeltaEntry>();

            if (pushDeltaOne != null)
            {
                stackedDeltas.Add(pushDeltaOne);
            }

            if (pushDeltaTwo != null)
            {
                stackedDeltas.Add(pushDeltaTwo);
            }
        }
    }
}