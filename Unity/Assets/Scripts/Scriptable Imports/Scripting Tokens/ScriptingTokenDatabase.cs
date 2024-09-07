namespace SFDDCards.ScriptingTokens
{
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class ScriptingTokenDatabase
    {
        public static List<IScriptingToken> AllTokenPrototypes = new List<IScriptingToken>()
        {
            new DamageScriptingToken(),
        };

        public static bool TryGetScriptingTokenMatch(string input, out IScriptingToken match)
        {
            foreach (IScriptingToken token in AllTokenPrototypes)
            {
                if (token.GetTokenIfMatch(input, out match))
                {
                    return true;
                }
            }

            match = null;
            return false;
        }

        public static List<IScriptingToken> GetAllTokens(string input)
        {
            List<IScriptingToken> tokens = new List<IScriptingToken>();

            MatchCollection tagMatches = Regex.Matches(input, @"(\[.*?\])+");
            foreach (Match curTagMatch in tagMatches)
            {
                if (!TryGetScriptingTokenMatch(curTagMatch.Value, out IScriptingToken token))
                {
                    Debug.LogError($"Failed to parse token! {curTagMatch.Value}");
                    continue;
                }

                tokens.Add(token);
            }

            return tokens;
        }
    }
}