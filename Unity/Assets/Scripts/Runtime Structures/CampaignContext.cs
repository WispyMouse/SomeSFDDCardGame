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
            NonCombatEncounter = 5,
            MakingRouteChoice = 6,
            Victory = 7
        }

        public enum NonCombatEncounterStatus
        {
            NotInNonCombatEncounter = 0,
            AllowedToLeave = 1
        }

        public readonly Deck CampaignDeck = new Deck();
        public CombatContext CurrentCombatContext { get; private set; } = null;
        public EvaluatedEncounter CurrentEncounter { get; private set; } = null;
        public readonly Player CampaignPlayer;
        public CampaignRoute OnRoute { get; private set; } = null;
        public int CampaignRouteNodeIndex { get; private set; } = -1;

        public GameplayCampaignState CurrentGameplayCampaignState { get; private set; } = GameplayCampaignState.NotStarted;
        public NonCombatEncounterStatus CurrentNonCombatEncounterStatus { get; private set; } = NonCombatEncounterStatus.NotInNonCombatEncounter;

        public Reward PendingRewards { get; set; } = null;

        public CampaignContext(RunConfiguration runConfig)
        {
            this.CampaignPlayer = new Player(runConfig.StartingMaximumHealth);

            foreach (string startingCard in runConfig.StartingDeck)
            {
                this.CampaignDeck.AddCardToDeck(CardDatabase.GetModel(startingCard));
            }
        }

        public void AddCardToDeck(Card toAdd)
        {
            this.CampaignDeck.AddCardToDeck(toAdd);
        }

        public void LeaveCurrentCombat()
        {
            if (this.CurrentCombatContext != null && this.CurrentCombatContext.BasedOnEncounter != null)
            {
                this.PendingRewards = this.CurrentCombatContext.Rewards;
            }

            this.CurrentCombatContext = null;
        }

        public void StartNextRoomFromEncounter(EvaluatedEncounter basedOn)
        {
            this.CurrentEncounter = basedOn;

            if (basedOn.BasedOn.IsShopEncounter)
            {
                this.LeaveCurrentCombat();
                this.SetCampaignState(GameplayCampaignState.NonCombatEncounter, NonCombatEncounterStatus.AllowedToLeave);
                return;
            }

            this.CurrentCombatContext = new CombatContext(this, basedOn);
            this.SetCampaignState(GameplayCampaignState.InCombat);
        }

        public void SetCampaignState(GameplayCampaignState toState, NonCombatEncounterStatus nonCombatState = NonCombatEncounterStatus.NotInNonCombatEncounter)
        {
            this.CurrentGameplayCampaignState = toState;
            this.CurrentNonCombatEncounterStatus = nonCombatState;

            if (toState != GameplayCampaignState.InCombat)
            {
                this.CampaignPlayer.AppliedStatusEffects.Clear();
            }

            if (toState == GameplayCampaignState.ClearedRoom && this.CurrentEncounter != null && this.CurrentCombatContext.Enemies.Count == 0)
            {
                this.LeaveCurrentCombat();
            }

            if (toState == GameplayCampaignState.MakingRouteChoice)
            {
                this.CampaignRouteNodeIndex++;

                if (this.OnRoute != null && this.CampaignRouteNodeIndex >= this.OnRoute.Nodes.Count)
                {
                    this.SetCampaignState(GameplayCampaignState.Victory);
                }
            }

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }

        public void SetRoute(RunConfiguration configuration, RouteImport routeToStart)
        {
            this.OnRoute = new CampaignRoute(configuration, routeToStart);
        }

        public void MakeChoiceNodeDecision(ChoiceNodeOption chosen)
        {
            chosen.WasSelected = true;
            this.CurrentEncounter = chosen.WillEncounter;
            this.StartNextRoomFromEncounter(chosen.WillEncounter);
            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }

        public ChoiceNode GetCampaignCurrentNode()
        {
            if (this.OnRoute == null || this.OnRoute.Nodes.Count <= this.CampaignRouteNodeIndex)
            {
                return null;
            }

            return this.OnRoute.Nodes[this.CampaignRouteNodeIndex];
        }

        public void ClearCombatPersistenceStatuses()
        {
            if (this.CampaignPlayer == null)
            {
                return;
            }

            foreach (AppliedStatusEffect effect in new List<AppliedStatusEffect>(this.CampaignPlayer.AppliedStatusEffects))
            {
                if (effect.BasedOnStatusEffect.Persistence == ImportModels.StatusEffectImport.StatusEffectPersistence.Combat)
                {
                    this.CampaignPlayer.AppliedStatusEffects.Remove(effect);
                }
            }
        }
    }
}