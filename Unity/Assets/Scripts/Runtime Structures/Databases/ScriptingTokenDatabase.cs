namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Conceptual;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class ScriptingTokenDatabase
    {
        public static List<IScriptingToken> AllTokenPrototypes = new List<IScriptingToken>()
        {
            // Effect Tokens
            new DamageScriptingToken(),
            new DrawScriptingToken(),
            new HealScriptingToken(),

            // Status Effect
            new ApplyStatusEffectStacksScriptingToken(),
            new RemoveStatusEffectStacksScriptingToken(),
            new SetStatusEffectStacksScriptingToken(),
            new DrainBothScriptingToken(),

            // Currency
            new SetCurrencyScriptingToken(),
            new ModCurrencyScriptingToken(),

            // Meta Token
            new ResetScriptingToken(),
            new LogIntScriptingToken(),
            new RequiresComparisonScriptingToken(),
            new IfTargetScriptingToken(),
            new MoveCardToZoneScriptingToken(),
            new GenerateCardScriptingToken(),
            new ShuffleScriptingToken(),
            new LeaveScriptingToken(),
            new GotoScriptingToken(),

            // Targeting Token
            new SetTargetScriptingToken(),
            new ChooseCardScriptingToken(),
            new CardTargetScriptingToken(),
            new ChooseRandomlyScriptingToken(),

            // Card and Artifact Manipulation Tokens
            new AddScriptingToken(),
            new RemoveScriptingToken(),

            // Element Token
            new GainElementScriptingToken(),
            new RequiresAtLeastElementScriptingToken(),
            new SetElementScriptingToken()
        };

        public static List<AliasScriptingToken> AllTokenAliases = new List<AliasScriptingToken>()
        {
            new DrainsElementScriptingToken(),
            new RemoveStacksScriptingToken(),
            new ClearStacksScriptingToken()
        };

        public static bool TryGetScriptingTokenMatch(string input, IEffectOwner owner, out List<IScriptingToken> matches)
        {
            List<string> adjustedInputs = new List<string>() { input };

            bool anythingFound = false;
            do
            {
                anythingFound = false;

                for  (int ii = 0; ii < adjustedInputs.Count; ii++)
                {
                    foreach (AliasScriptingToken alias in AllTokenAliases)
                    {
                        if (alias.GetTokensIfMatch(adjustedInputs[ii], owner, out List<string> result))
                        {
                            adjustedInputs.RemoveAt(ii);
                            for (int jj = result.Count - 1; jj >= 0; jj--)
                            {
                                adjustedInputs.Insert(ii, result[jj]);
                            }
                            anythingFound = true;
                            break;
                        }
                    }
                }
            } while (anythingFound);
            
            matches = new List<IScriptingToken>();

            foreach (string adjustedInput in adjustedInputs)
            {
                foreach (IScriptingToken token in AllTokenPrototypes)
                {
                    if (token.GetTokenIfMatch(adjustedInput, out IScriptingToken match))
                    {
                        matches.Add(match);
                    }
                }
            }
            
            return matches.Count > 0;
        }

        public static AttackTokenPile GetAllTokens(string input, IEffectOwner owner)
        {
            List<IScriptingToken> tokens = new List<IScriptingToken>();

            MatchCollection tagMatches = Regex.Matches(input, @"(\[.*?\])");
            foreach (Match curTagMatch in tagMatches)
            {
                if (!TryGetScriptingTokenMatch(curTagMatch.Value, owner, out List<IScriptingToken> newTokens))
                {
                    Debug.LogError($"Failed to parse token! {curTagMatch.Value}");
                    continue;
                }

                tokens.AddRange(newTokens);
            }

            return new AttackTokenPile(owner, tokens);
        }
    }
}