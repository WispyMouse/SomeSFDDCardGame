namespace SFDDCards.Evaluation.Actual
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using SFDDCards.Evaluation.Actual;

    public class GamestateDelta
    {
        public List<DeltaEntry> DeltaEntries { get; set; } = new List<DeltaEntry>();

        public void ApplyDelta(CampaignContext campaignContext)
        {
            foreach (DeltaEntry entry in DeltaEntries)
            {
                if (entry.IntensityKindType == TokenEvaluatorBuilder.IntensityKind.NumberOfCards)
                {
                    if (entry.NumberOfCardsRelationType == TokenEvaluatorBuilder.NumberOfCardsRelation.Draw)
                    {
                        campaignContext.CurrentCombatContext.PlayerCombatDeck.DealCards(entry.Intensity);
                    }
                }

                foreach (ElementResourceChange change in entry.ElementResourceChanges)
                {
                    campaignContext.CurrentCombatContext.ApplyElementResourceChange(entry.MadeFromBuilder, change);
                }
                
                entry.Target?.ApplyDelta(campaignContext, campaignContext.CurrentCombatContext, entry);

                foreach (Action<GamestateDelta> actionToExecute in entry.ActionsToExecute)
                {
                    actionToExecute.Invoke(this);
                }
            }
        }

        public void AppendDelta(GamestateDelta delta)
        {
            this.DeltaEntries.AddRange(delta.DeltaEntries);
        }
    }
}