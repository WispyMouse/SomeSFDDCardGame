using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class RequiresAtLeastElementScriptingToken : IScriptingToken
    {
        public string Element { get; private set; }
        public int Amount { get; private set; }

        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            if (tokenBuilder.ElementRequirements.ContainsKey(this.Element))
            {
                tokenBuilder.ElementRequirements[this.Element] = this.Amount;
            }
            else
            {
                tokenBuilder.ElementRequirements.Add(this.Element, this.Amount);
            }
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[REQUIRESATLEASTELEMENT: (\d+) (\w+)\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            RequiresAtLeastElementScriptingToken typedMatch = new RequiresAtLeastElementScriptingToken();
            typedMatch.Amount = int.Parse(regexMatch.Groups[1].Value);
            typedMatch.Element = regexMatch.Groups[2].Value.ToUpper();
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}