namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CampaignContext
    {
        public enum GameplayCampaignState
        {
            NotStarted = 0,
            ClearedRoom = 1,
            InCombat = 2,
            Defeat = 3,
            EnteringRoom = 4,
            NonCombatEncounter = 5
        }

        public enum NonCombatEncounterStatus
        {
            NotInNonCombatEncounter = 0,
            AllowedToLeave = 1
        }

        public readonly Deck CampaignDeck = new Deck();
        public CombatContext CurrentCombatContext { get; private set; } = null;
        public Encounter CurrentEncounter { get; private set; } = null;
        public readonly Player CampaignPlayer;

        public GameplayCampaignState CurrentGameplayCampaignState { get; private set; } = GameplayCampaignState.NotStarted;
        public NonCombatEncounterStatus CurrentNonCombatEncounterStatus { get; private set; } = NonCombatEncounterStatus.NotInNonCombatEncounter;

        public CampaignContext(RunConfiguration runConfig)
        {
            this.CampaignPlayer = new Player(runConfig.StartingMaximumHealth);
        }

        public void AddCardToDeck(Card toAdd)
        {
            this.CampaignDeck.AddCardToDeck(toAdd);
        }

        public void LeaveCurrentCombat()
        {
            this.CurrentCombatContext = null;
        }

        public void StartNextRoomFromEncounter(Encounter basedOn)
        {
            this.CurrentEncounter = basedOn;

            if (basedOn.IsShopEncounter)
            {
                this.LeaveCurrentCombat();
                this.CurrentGameplayCampaignState = GameplayCampaignState.NonCombatEncounter;
                this.CurrentNonCombatEncounterStatus = NonCombatEncounterStatus.AllowedToLeave;
                return;
            }

            this.CurrentCombatContext = new CombatContext(this, basedOn);
        }

        public void SetCampaignState(GameplayCampaignState toState, NonCombatEncounterStatus nonCombatState = NonCombatEncounterStatus.NotInNonCombatEncounter)
        {
            this.CurrentGameplayCampaignState = toState;
            this.CurrentNonCombatEncounterStatus = nonCombatState;
        }
    }
}