using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class GainElementScriptingToken : IScriptingToken
    {
        public string Element { get; private set; }
        public int Amount { get; private set; }

        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ElementResourceChanges.Add(new ElementResourceChange() { Element = this.Element, GainOrLoss = this.Amount });
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[GAINELEMENT: (\d+) (\w+)\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            GainElementScriptingToken typedMatch = new GainElementScriptingToken();
            typedMatch.Amount = int.Parse(regexMatch.Groups[1].Value);
            typedMatch.Element = regexMatch.Groups[2].Value;
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}