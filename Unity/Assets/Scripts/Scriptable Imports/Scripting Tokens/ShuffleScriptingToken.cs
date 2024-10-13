namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class ShuffleScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public override string ScriptingTokenIdentifier { get; } = "SHUFFLE";

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            applyingDuringEntry.ActionsToExecute.Add((DeltaEntry entryArg) => 
            {
                entryArg.FromCampaign?.CurrentCombatContext?.PlayerCombatDeck?.ShuffleDeck();
            });
            stackedDeltas = null;
        }

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.RealizedOperationScriptingToken = this;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            return "Shuffle";
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            return "Shuffle";
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = new ShuffleScriptingToken();
            return true;
        }
    }
}