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
            foreach (string window in this.BasedOnStatusEffect.WindowResponders.Keys)
            {
                ReactionWindowSubscription subscription = KnownReactionWindows.ParseWindow(window, this);
                if (subscription != null)
                {
                    context.SubscribeToReactionWindow(this, subscription);
                }
            }
        }

        public bool TryGetReactionEvents(CampaignContext campaignContext, ReactionWindowContext reactionContext, out List<WindowResponse> responses)
        {
            if (!this.BasedOnStatusEffect.WindowResponders.TryGetValue(reactionContext.TimingWindowId, out List<WindowResponder> responders))
            {
                responses = null;
                return false;
            }

            responses = new List<WindowResponse>();
            foreach (WindowResponder responder in responders)
            {
                responses.Add(new WindowResponse(responder, reactionContext, this));
            }

            return responses.Count > 0;
        }

        public EffectDescription DescribeStatusEffect()
        {
            return this.BasedOnStatusEffect.DescribeStatusEffect();
        }
    }
}