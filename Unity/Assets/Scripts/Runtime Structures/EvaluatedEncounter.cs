namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class EvaluatedEncounter
    {
        public readonly EncounterModel BasedOn;
        public Reward Rewards;
        public List<Enemy> Enemies = new List<Enemy>();

        public EvaluatedEncounter(EncounterModel basedOn)
        {
            this.BasedOn = basedOn;

            if (basedOn.RewardsModel != null)
            {
                this.Rewards = RewardDatabase.SaturateReward(basedOn.RewardsModel);
            }

            foreach (string enemyId in basedOn.EnemiesInEncounterById)
            {
                EnemyModel model = EnemyDatabase.GetModel(enemyId);

                if (model == null)
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to get enemy based on id. {enemyId}", GlobalUpdateUX.LogType.RuntimeError);
                }

                this.Enemies.Add(new Enemy(model));
            }
        }

        public string GetName()
        {
            return this.BasedOn.Id;
        }

        public string GetDescription()
        {
            return this.BasedOn.Description;
        }

        public List<ShopEntry> GetShop(CampaignContext forCampaign)
        {
            if (this.BasedOn.Arguments.Count == 0)
            {
                return null;
            }

            List<ShopEntry> shopEntries = new List<ShopEntry>();
            RandomDecider<CardImport> cardDecider = new DoNotRepeatRandomDecider<CardImport>();

            List<string> remainingArguments = new List<string>();

            foreach (string argList in this.BasedOn.Arguments)
            {
                // Try to parse each of the incoming values as a card, first
                // If something doesn't parse, note it for later
                string[] splitList = argList.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string splitArg in splitList)
                {
                    Card chosenCard = CardDatabase.GetModel(splitArg, cardDecider);
                    if (chosenCard == null)
                    {
                        remainingArguments.Add(splitArg);
                        continue;
                    }

                    shopEntries.Add(new ShopEntry() { GainedCard = chosenCard, GainedAmount = new ConstantNumericEvaluatableValue(1) });
                }

                // Now iterate over those values to determine if there's any artifacts we should award
                foreach (string argThatMightBeAnArtifact in splitList)
                {
                    if (StatusEffectDatabase.TryGetStatusEffectById(argThatMightBeAnArtifact, out StatusEffect artifact))
                    {
                        shopEntries.Add(new ShopEntry() { GainedEffect = artifact, GainedAmount = new ConstantNumericEvaluatableValue(1) });
                    }
                }
            }

            foreach (ShopEntry shopEntry in shopEntries)
            {
                shopEntry.Costs = forCampaign.GetPriceForItem(shopEntry);
            }

            return shopEntries;
        }
    }
}