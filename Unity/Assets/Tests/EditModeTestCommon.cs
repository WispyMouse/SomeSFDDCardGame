namespace SFDDCards.Tests.EditMode
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;

    public static class EditModeTestCommon
    {
        public enum ParseKind
        {
            Card,
            StatusEffect
        }

        public struct ParseFromFileTestData
        {
            public string Id;
            public string ExpectedParse;
            public ParseKind ParseKind;

            public ParseFromFileTestData(string id, string expectedParse, ParseKind parseKind)
            {
                this.Id = id;
                this.ExpectedParse = expectedParse;
                this.ParseKind = parseKind;
            }

            public string FindFileLocation()
            {
                string fileName = $"{Id}.{GetImportForType()}";
                string[] files = Directory.GetFiles(Application.streamingAssetsPath, fileName, SearchOption.AllDirectories);
                if (files.Length != 1)
                {
                    throw new FileNotFoundException($"Could not find file {fileName}");
                }
                return files[0];
            }

            public string GetImportForType()
            {
                switch (ParseKind)
                {
                    case ParseKind.Card:
                        return ".cardImport";
                    case ParseKind.StatusEffect:
                        return ".statusImport";
                    default:
                        return "";
                }
            }
        }

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

            List<string> idsToEncounter = new List<string>();
            for (int ii = 0; ii < numberOfPunchingBags; ii++)
            {
                idsToEncounter.Add(punchingBag.Id);
            }

            EncounterModel toEncounter = new EncounterModel(
                new EncounterImport()
                {
                    Id = nameof(GetEncounterWithPunchingBags) + numberOfPunchingBags.ToString() + amountOfHealth.ToString(),
                    Name = nameof(GetEncounterWithPunchingBags),
                    EnemyIds = idsToEncounter
                });

            return toEncounter;
        }

        public static CampaignContext GetBlankCampaignContext()
        {
            RunConfiguration blankConfiguration = GetDefaultRunConfiguration();
            CampaignContext newContext = new CampaignContext(new CampaignRoute(new RunConfiguration(), new RouteImport()));
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

        public static void ApplyStatusEffectStacks(string toApply, CampaignContext campaignContext, CombatContext combatContext, ICombatantTarget toTarget, int mod)
        {
            toTarget.ApplyDelta(campaignContext,
                combatContext,
                new DeltaEntry(campaignContext, combatContext.CombatPlayer, toTarget)
                {
                    IntensityKindType = TokenEvaluatorBuilder.IntensityKind.ApplyStatusEffect,
                    Intensity = mod,
                    StatusEffect = StatusEffectDatabase.GetModel(toApply)
                });

            GlobalSequenceEventHolder.SynchronouslyResolveAllEvents();
        }

        public static void AddBlankCardsToPlayerDeck(CombatDeck deck, int numberOfCards)
        {
            for (int ii = 0; ii < numberOfCards; ii++)
            {
                Guid cardId = Guid.NewGuid();
                CardImport import = new CardImport()
                {
                    Id = cardId.ToString(),
                    Name = cardId.ToString(),
                    EffectScript = ""
                };

                Card derivedCard = new Card(import);
                deck.CardsCurrentlyInDeck.Add(derivedCard);
            }
        }
    }
}