using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class LaunchScriptingToken : IScriptingToken
    {
        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ShouldLaunch = true;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[LAUNCH\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            LaunchScriptingToken typedMatch = new LaunchScriptingToken();
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}