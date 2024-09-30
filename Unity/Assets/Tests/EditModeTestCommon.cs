namespace SFDDCards.Tests.EditMode
{
    using NUnit.Framework;
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
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
            StatusEffect,
            Element
        }

        public struct ParseFromFileTestData
        {
            public string Id;
            public string ExpectedParsedValue;
            public ParseKind ParseKind;

            public ParseFromFileTestData(string id, string expectedParse, ParseKind parseKind)
            {
                this.Id = id;
                this.ExpectedParsedValue = expectedParse;
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

            public override string ToString()
            {
                return $"{this.Id}: {this.ExpectedParsedValue}";
            }
        }

        public class DependentFile
        {
            public string TypelessFilepath;
            public ParseKind ParseKind;

            public DependentFile(string id, ParseKind parseKind)
            {
                this.TypelessFilepath = id;
                this.ParseKind = parseKind;
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

        public static void AssertCardParsing(string attackTokens, string expectedEvaluation, ReactionWindowContext? context)
        {
            CardImport import = new CardImport()
            {
                Id = nameof(AssertCardParsing),
                Name = nameof(AssertCardParsing),
                EffectScript = attackTokens
            };

            Card derivedCard = new Card(import);
            AssertCardParsing(derivedCard, expectedEvaluation, context);
        }

        public static void AssertCardParsing(Card card, string expectedEvaluation, ReactionWindowContext? context)
        {
            EffectDescription description = card.GetDescription(context);

            List<string> descriptionTexts = description.DescriptionText;
            Assert.AreEqual(1, descriptionTexts.Count, "Scripts should only parse in to one description text when validated using this function.");
            Assert.AreEqual(expectedEvaluation, descriptionTexts[0], "Script should parse out to expected value.");
        }

        public static void AssertStatusEffectParsing(string attackTokens, string expectedEvaluation, string window, StatusEffect effectOwner)
        {
            AttackTokenPile pile = ScriptingTokens.ScriptingTokenDatabase.GetAllTokens(attackTokens, effectOwner);
            effectOwner.EffectTokens.Clear();
            effectOwner.EffectTokens.Add(window, new List<AttackTokenPile>() { pile });
            AssertStatusEffectParsing(effectOwner, expectedEvaluation);
        }

        public static void AssertStatusEffectParsing(StatusEffect statusEffect, string expectedEvaluation)
        {
            string resolvedDescription = statusEffect.DescribeStatusEffect().BreakDescriptionsIntoString();
            Assert.AreEqual(expectedEvaluation, resolvedDescription, "Script should parse out to expected value.");
        }
    }
}