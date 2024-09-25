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

        public IEffectOwner Owner { get; }

        public AttackTokenPile(IEffectOwner owner, List<IScriptingToken> attackTokens, Dictionary<Element, int> baseElementGain = null)
        {
            this.Owner = owner;
            this.AttackTokens = attackTokens;
            this.BaseElementGain = baseElementGain == null ? new Dictionary<Element, int>() : baseElementGain;
        }
    }
}