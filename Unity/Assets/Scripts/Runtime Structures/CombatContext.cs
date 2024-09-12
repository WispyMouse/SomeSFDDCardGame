namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CombatContext
    {
        public enum TurnStatus
        {
            NotInCombat = 0,
            PlayerTurn = 1,
            EnemyTurn = 2
        }

        public Dictionary<string, int> ElementResourceCounts { get; private set; } = new Dictionary<string, int>();
        public TurnStatus CurrentTurnStatus { get; private set; } = TurnStatus.NotInCombat;

        public readonly CombatDeck PlayerCombatDeck;

        public readonly Encounter BasedOnEncounter;
        public readonly List<Enemy> Enemies = new List<Enemy>();

        public CombatContext(CampaignContext fromCampaign, Encounter basedOnEncounter)
        {
            this.BasedOnEncounter = basedOnEncounter;
            this.PlayerCombatDeck = new CombatDeck(fromCampaign.CampaignDeck);
            this.PlayerCombatDeck.ShuffleEntireDeck();
        }

        public bool MeetsElementRequirement(string element, int minimumCount)
        {
            if (minimumCount <= 0)
            {
                return true;
            }

            if (this.ElementResourceCounts.TryGetValue(element, out int amountHeld))
            {
                if (amountHeld < minimumCount)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public void ApplyElementResourceChange(ElementResourceChange toChange)
        {
            if (this.ElementResourceCounts.TryGetValue(toChange.Element, out int currentAmount))
            {
                int newAmount = Mathf.Max(0, currentAmount + toChange.GainOrLoss);

                if (newAmount > 0)
                {
                    this.ElementResourceCounts[toChange.Element] = newAmount;
                }
                else
                {
                    this.ElementResourceCounts.Remove(toChange.Element);
                }
            }
            else
            {
                if (toChange.GainOrLoss > 0)
                {
                    this.ElementResourceCounts.Add(toChange.Element, toChange.GainOrLoss);
                }
            }

            UpdateUXGlobalEvent.UpdateUXEvent.Invoke();
        }

        public void EndCurrentTurnAndChangeTurn(TurnStatus toTurn)
        {
            this.CurrentTurnStatus = toTurn;

            UpdateUXGlobalEvent.UpdateUXEvent.Invoke();
        }
    }
}