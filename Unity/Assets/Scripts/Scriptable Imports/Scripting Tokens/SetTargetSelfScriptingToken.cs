using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class SetTargetSelfScriptingToken : IScriptingToken
    {
        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Target = tokenBuilder.User;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[SETTARGETSELF\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            SetTargetSelfScriptingToken typedMatch = new SetTargetSelfScriptingToken();
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}