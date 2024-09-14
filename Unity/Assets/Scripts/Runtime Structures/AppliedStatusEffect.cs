namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AppliedStatusEffect : IReactionWindowReactor
    {
        public readonly StatusEffect BasedOnStatusEffect;
        public readonly Combatant Owner;
        public int Stacks { get; set; }

        public AppliedStatusEffect(Combatant owner, StatusEffect basedOnEffect, int stacks = 0)
        {
            this.Owner = owner;
            this.BasedOnStatusEffect = basedOnEffect;
            this.Stacks = stacks;
        }

        public void SetSubscriptions(CombatContext context)
        {
            foreach (string window in this.BasedOnStatusEffect.EffectTokens.Keys)
            {
                context.SubscribeToReactionWindow(this, KnownReactionWindows.ParseWindow(window, this));
            }
        }

        public bool TryGetReactionEvents(CombatContext combatContext, ReactionWindowContext reactionContext, out List<GameplaySequenceEvent> events)
        {
            events = null;

            if (!this.BasedOnStatusEffect.EffectTokens.TryGetValue(reactionContext.TimingWindowId, out List<List<ScriptingTokens.IScriptingToken>> tokens))
            {
                return false;
            }

            events = new List<GameplaySequenceEvent>();

            foreach (List<IScriptingToken> tokenList in tokens)
            {
                events.Add(new GameplaySequenceEvent(() => combatContext.StatusEffectHappeningProc(new StatusEffectHappening(this, tokenList)), null));
            };

            return true;
        }
    }
}