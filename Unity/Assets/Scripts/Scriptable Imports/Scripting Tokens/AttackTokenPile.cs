namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;

    public class AttackTokenPile : IAttackTokenHolder
    {
        public List<IScriptingToken> AttackTokens { get; private set; } = new List<IScriptingToken>();

        public Dictionary<Element, int> BaseElementGain { get; private set; } = new Dictionary<Element, int>();

        public AttackTokenPile(List<IScriptingToken> attackTokens, Dictionary<Element, int> baseElementGain = null)
        {
            this.AttackTokens = attackTokens;
            this.BaseElementGain = baseElementGain == null ? new Dictionary<Element, int>() : baseElementGain;
        }
    }
}