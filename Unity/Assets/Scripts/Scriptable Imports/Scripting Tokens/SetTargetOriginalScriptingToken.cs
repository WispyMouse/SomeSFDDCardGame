using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class SetTargetOriginalScriptingToken : IScriptingToken
    {
        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Target = tokenBuilder.TopOfEffectTarget;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[SETTARGETORIGINAL\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            SetTargetOriginalScriptingToken typedMatch = new SetTargetOriginalScriptingToken();
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}