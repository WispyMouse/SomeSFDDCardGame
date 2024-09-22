namespace SFDDCards
{
    using SFDDCards.ScriptingTokens;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class AppliedStatusEffect : IReactionWindowReactor, IStatusEffect
    {
        public readonly StatusEffect BasedOnStatusEffect;
        public readonly Combatant Owner;
        public int Stacks { get; set; }

        public Dictionary<Element, int> BaseElementGain => new Dictionary<Element, int>();

        public AppliedStatusEffect(Combatant owner, StatusEffect basedOnEffect, int stacks = 0)
        {
            this.Owner = owner;
            this.BasedOnStatusEffect = basedOnEffect;
            this.Stacks = stacks;
        }

        public void SetSubscriptions(CampaignContext context)
        {
            foreach (string window in this.BasedOnStatusEffect.EffectTokens.Keys)
            {
                context.SubscribeToReactionWindow(this, KnownReactionWindows.ParseWindow(window, this));
            }
        }

        public bool TryGetReactionEvents(CampaignContext campaignContext, ReactionWindowContext reactionContext, out List<GameplaySequenceEvent> events)
        {
            events = null;

            if (!this.BasedOnStatusEffect.EffectTokens.TryGetValue(reactionContext.TimingWindowId, out List<AttackTokenPile> tokens))
            {
                return false;
            }

            events = new List<GameplaySequenceEvent>();

            foreach (AttackTokenPile tokenList in tokens)
            {
                events.Add(new GameplaySequenceEvent(() => campaignContext.StatusEffectHappeningProc(new StatusEffectHappening(this, tokenList.AttackTokens)), null));
            };

            return true;
        }

        public EffectDescription DescribeStatusEffect()
        {
            return this.BasedOnStatusEffect.DescribeStatusEffect();
        }
    }
}