namespace SFDDCards.ScriptingTokens
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public interface IScriptingToken
    {
        bool GetTokenIfMatch(string tokenString, out IScriptingToken match);

        void ApplyToken(TokenEvaluatorBuilder tokenBuilder);
        bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target);
    }
}
