namespace SFDDCards.Evaluation.Conceptual
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using static SFDDCards.Evaluation.Actual.TokenEvaluatorBuilder;

    public interface IRealizedOperationScriptingToken : IScriptingToken
    {
        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId);
        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder);
        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas);
    }
}