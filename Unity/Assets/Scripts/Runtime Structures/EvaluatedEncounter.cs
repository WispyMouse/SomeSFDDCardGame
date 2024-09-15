namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class EvaluatedEncounter
    {
        public readonly EncounterModel BasedOn;

        public EvaluatedEncounter(EncounterModel basedOn)
        {
            this.BasedOn = basedOn;
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
            RandomDecider<Card> cardDecider = new RandomDecider<Card>();
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