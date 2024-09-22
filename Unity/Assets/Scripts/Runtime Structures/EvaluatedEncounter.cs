namespace SFDDCards
{
    using SFDDCards.ImportModels;
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

        public List<Card> GetCards()
        {
            if (this.BasedOn.Arguments.Count == 0)
            {
                return null;
            }

            List<Card> cards = new List<Card>();
            RandomDecider<CardImport> cardDecider = new DoNotRepeatRandomDecider<CardImport>();
            foreach (string argList in this.BasedOn.Arguments)
            {
                string[] splitList = argList.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string splitArg in splitList)
                {
                    Card chosenCard = CardDatabase.GetModel(splitArg, cardDecider);
                    if (chosenCard == null)
                    {
                        continue;
                    }
                    cards.Add(chosenCard);
                }
            }
            return cards;
        }
    }
}