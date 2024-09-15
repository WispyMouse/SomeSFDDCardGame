namespace SFDDCards.Tests.EditMode
{
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public static class EditModeTestCommon
    {
        public static EncounterModel GetEncounterWithPunchingBags(int numberOfPunchingBags, int amountOfHealth)
        {
            EnemyImport punchingBag = new EnemyImport()
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"Punching Bag {amountOfHealth}",
                MaximumHealth = amountOfHealth,
                Attacks = new List<EnemyAttackImport>()
            };

            EnemyDatabase.AddEnemyToDatabase(punchingBag);

            EncounterModel toEncounter = new EncounterModel();

            toEncounter.Id = nameof(GetEncounterWithPunchingBags) + numberOfPunchingBags.ToString() + amountOfHealth.ToString();

            for (int ii = 0; ii < numberOfPunchingBags; ii++)
            {
                toEncounter.EnemiesInEncounterById.Add(punchingBag.Id);
            }

            return toEncounter;
        }

        public static CampaignContext GetBlankCampaignContext()
        {
            RunConfiguration blankConfiguration = GetDefaultRunConfiguration();

            // Empty out the starting deck, so tests have a guaranteed hand of cards
            blankConfiguration.StartingDeck = new List<string>();

            CampaignContext newContext = new CampaignContext(blankConfiguration, null);
            return newContext;
        }

        public static RunConfiguration GetDefaultRunConfiguration()
        {
            return new RunConfiguration();
        }

        public static void PlayerDrawsDeck(CombatContext combatContext)
        {
            combatContext.PlayerCombatDeck.DealCards(combatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Count);
        }

        public static void ApplyStatusEffectStacks(string toApply, CombatContext combatContext, ICombatantTarget toTarget, int mod)
        {
            toTarget.ApplyDelta(combatContext, new DeltaEntry()
            {
                Target = toTarget,
                IntensityKindType = TokenEvaluatorBuilder.IntensityKind.StatusEffect,
                Intensity = mod,
                StatusEffect = StatusEffectDatabase.GetModel(toApply)
            });

            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();
        }
    }
}