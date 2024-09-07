using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class DamageScriptingToken : IScriptingToken
    {
        public int Damage { get; private set; }

        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Damage;
            tokenBuilder.Intensity = Damage;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[DAMAGE: (\d+)\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            DamageScriptingToken typedMatch = new DamageScriptingToken();
            typedMatch.Damage = int.Parse(regexMatch.Groups[1].Value);
            match = typedMatch;

            return true;
        }
    }
}