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

        public override bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }

        public override bool RequiresTarget()
        {
            return false;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            if (this.ReduceArgumentOne.ToLower() == "intensity" && reactionWindowId == KnownReactionWindows.IncomingDamage)
            {
                return $"Damage first subtracts from {this.ReduceArgumentTwo} before subtracting from health.";
            }
            
            return $"Reduce {this.ReduceArgumentOne} by {this.ReduceArgumentTwo}.";
        }

        public void ApplyToDelta(DeltaEntry toApplyTo, out List<DeltaEntry> stackedDeltas)
        {
            int argumentOneValue = toApplyTo.GetArgumentValue(this.ReduceArgumentOne);
            int argumentTwoValue = toApplyTo.GetArgumentValue(this.ReduceArgumentTwo);

            int argumentOneFinalValue = argumentOneValue > argumentTwoValue ? argumentOneValue - argumentTwoValue : 0;
            int argumentTwoFinalValue = argumentTwoValue > argumentOneValue ? argumentTwoValue - argumentOneValue : 0;

            DeltaEntry pushDeltaOne = toApplyTo.SetArgumentValue(ReduceArgumentTwo, argumentTwoFinalValue);
            DeltaEntry pushDeltaTwo = toApplyTo.SetArgumentValue(ReduceArgumentTwo, argumentTwoFinalValue);

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