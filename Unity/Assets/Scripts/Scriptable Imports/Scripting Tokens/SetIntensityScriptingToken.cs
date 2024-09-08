using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class SetIntensityScriptingToken : IScriptingToken
    {
        public int Intensity { get; private set; }

        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.Intensity = Intensity;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[SETINTENSITY: (\d+)\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            SetIntensityScriptingToken typedMatch = new SetIntensityScriptingToken();
            typedMatch.Intensity = int.Parse(regexMatch.Groups[1].Value);
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}