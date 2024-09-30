namespace SFDDCards.ScriptingTokens
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;

    public interface IScriptingToken
    {
        bool GetTokenIfMatch(string tokenString, out IScriptingToken match);

        void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder);
        bool SkipDescribingMe { get; }
    }
}
