using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class HealScriptingToken : IScriptingToken
    {
        public int Healing { get; private set; }

        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.Heal;
            tokenBuilder.Intensity = Healing;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[HEAL: (\d+)\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            HealScriptingToken typedMatch = new HealScriptingToken();
            typedMatch.Healing = int.Parse(regexMatch.Groups[1].Value);
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}