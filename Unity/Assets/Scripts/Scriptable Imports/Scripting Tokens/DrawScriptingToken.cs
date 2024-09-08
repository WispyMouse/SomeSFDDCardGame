using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class DrawScriptingToken : IScriptingToken
    {
        public int Amount { get; private set; }

        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.NumberOfCards;
            tokenBuilder.NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.Draw;
            tokenBuilder.Intensity = Amount;
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[DRAW: (\d+)\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            DrawScriptingToken typedMatch = new DrawScriptingToken();
            typedMatch.Amount = int.Parse(regexMatch.Groups[1].Value);
            match = typedMatch;

            return true;
        }
    }
}