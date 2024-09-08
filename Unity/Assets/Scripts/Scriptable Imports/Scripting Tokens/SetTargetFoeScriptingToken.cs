using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class SetTargetFoeScriptingToken : IScriptingToken
    {
        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Target = null;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[SETTARGETFOE\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            SetTargetFoeScriptingToken typedMatch = new SetTargetFoeScriptingToken();
            match = typedMatch;

            return true;
        }
    }
}