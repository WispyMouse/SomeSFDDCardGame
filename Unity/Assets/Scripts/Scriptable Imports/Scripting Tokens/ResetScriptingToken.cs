using SFDDCards.ScriptingTokens.EvaluatableValues;
using System.Text.RegularExpressions;

namespace SFDDCards.ScriptingTokens
{
    public class ResetScriptingToken : IScriptingToken
    {
        public void ApplyToken(TokenEvaluatorBuilder tokenBuilder)
        {
            tokenBuilder.ElementRequirements.Clear();
            tokenBuilder.Target = new OriginalTargetEvaluatableValue();
            tokenBuilder.IntensityKindType = TokenEvaluatorBuilder.IntensityKind.None;
            tokenBuilder.NumberOfCardsRelationType = TokenEvaluatorBuilder.NumberOfCardsRelation.None;
            tokenBuilder.Intensity = null;
            tokenBuilder.ElementResourceChanges.Clear();
        }

        public bool GetTokenIfMatch(string tokenString, out IScriptingToken match)
        {
            Match regexMatch = Regex.Match(tokenString, @"^\[RESET\]$");
            if (!regexMatch.Success)
            {
                match = null;
                return false;
            }

            ResetScriptingToken typedMatch = new ResetScriptingToken();
            match = typedMatch;

            return true;
        }

        public bool IsHarmfulToTarget(ICombatantTarget user, ICombatantTarget target)
        {
            return false;
        }
    }
}