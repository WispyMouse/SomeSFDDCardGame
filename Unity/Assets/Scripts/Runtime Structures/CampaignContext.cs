namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
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

        private readonly Dictionary<IReactionWindowReactor, HashSet<ReactionWindowSubscription>> ReactorsToSubscriptions = new Dictionary<IReactionWindowReactor, HashSet<ReactionWindowSubscription>>();
        private readonly Dictionary<string, List<ReactionWindowSubscription>> WindowsToReactors = new Dictionary<string, List<ReactionWindowSubscription>>();

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
                this.ClearCombatPersistenceStatuses();
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

        public void CheckAndApplyReactionWindow(ReactionWindowContext context)
        {
            if (!this.TryGetReactionWindowSequenceEvents(context, out List<GameplaySequenceEvent> eventsThatWouldFollow))
            {
                return;
            }

            GlobalSequenceEventHolder.PushSequencesToTop(eventsThatWouldFollow.ToArray());
        }

        public bool TryGetReactionWindowSequenceEvents(ReactionWindowContext context, out List<GameplaySequenceEvent> eventsThatWouldFollow)
        {
            eventsThatWouldFollow = null;

            if (this.WindowsToReactors.TryGetValue(context.TimingWindowId, out List<ReactionWindowSubscription> reactors))
            {
                foreach (ReactionWindowSubscription reactor in reactors)
                {
                    if (reactor != null && reactor.ShouldApply(context) && reactor.Reactor.TryGetReactionEvents(this, context, out List<GameplaySequenceEvent> events))
                    {
                        if (eventsThatWouldFollow == null)
                        {
                            eventsThatWouldFollow = new List<GameplaySequenceEvent>();
                        }

                        eventsThatWouldFollow.AddRange(events);
                    }
                }
            }

            if (eventsThatWouldFollow == null)
            {
                return false;
            }

            return true;
        }

        public void SubscribeToReactionWindow(IReactionWindowReactor reactor, ReactionWindowSubscription subscription)
        {
            if (!this.WindowsToReactors.TryGetValue(subscription.ReactionWindowId.ToLower(), out List<ReactionWindowSubscription> reactorsList))
            {
                reactorsList = new List<ReactionWindowSubscription>();
                this.WindowsToReactors.Add(subscription.ReactionWindowId.ToLower(), reactorsList);
            }

            reactorsList.Add(subscription);

            if (!this.ReactorsToSubscriptions.TryGetValue(reactor, out HashSet<ReactionWindowSubscription> subscriptions))
            {
                subscriptions = new HashSet<ReactionWindowSubscription>();
                this.ReactorsToSubscriptions.Add(reactor, subscriptions);
            }

            subscriptions.Add(subscription);
        }

        public void UnsubscribeReactor(IReactionWindowReactor reactor)
        {
            if (this.ReactorsToSubscriptions.TryGetValue(reactor, out HashSet<ReactionWindowSubscription> reactions))
            {
                foreach (ReactionWindowSubscription reactionWindow in reactions)
                {
                    this.WindowsToReactors[reactionWindow.ReactionWindowId].Remove(reactionWindow);
                }
                this.ReactorsToSubscriptions.Remove(reactor);
            }
        }

        public void StatusEffectHappeningProc(StatusEffectHappening happening)
        {
            GamestateDelta delta = ScriptTokenEvaluator.CalculateRealizedDeltaEvaluation(happening, this, happening.OwnedStatusEffect?.BasedOnStatusEffect, happening.OwnedStatusEffect?.Owner, this.CampaignPlayer, happening.Context);
            GlobalUpdateUX.LogTextEvent.Invoke(EffectDescriberDatabase.DescribeResolvedEffect(delta), GlobalUpdateUX.LogType.GameEvent);
            delta.ApplyDelta(this);

            this.CheckAllStateEffectsAndKnockouts();
        }

        public void CheckAllStateEffectsAndKnockouts()
        {
            if (this.CampaignPlayer.CurrentHealth <= 0)
            {
                this.PlayerDefeat();
                return;
            }

            if (this.CurrentCombatContext != null)
            {
                List<Enemy> enemies = new List<Enemy>(this.CurrentCombatContext.Enemies);
                foreach (Enemy curEnemy in enemies)
                {
                    if (curEnemy.ShouldBecomeDefeated && !curEnemy.DefeatHasBeenSignaled)
                    {
                        this.CurrentCombatContext.RemoveEnemy(curEnemy);
                    }
                }
            }

            if (this.CurrentNonCombatEncounterStatus == CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter && this.CurrentCombatContext.Enemies.Count == 0)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"There are no more enemies!", GlobalUpdateUX.LogType.GameEvent);
                this.SetCampaignState(CampaignContext.GameplayCampaignState.ClearedRoom);
                return;
            }

            GlobalUpdateUX.UpdateUXEvent?.Invoke();
        }


        private void PlayerDefeat()
        {
            foreach (AppliedStatusEffect effect in this.CampaignPlayer.AppliedStatusEffects)
            {
                this.UnsubscribeReactor(effect);
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"The player has run out of health! This run is over.", GlobalUpdateUX.LogType.GameEvent);
            this.SetCampaignState(CampaignContext.GameplayCampaignState.Defeat);
            this.UnsubscribeReactor(this.CampaignPlayer);

            GlobalSequenceEventHolder.StopAllSequences();
        }
    }
}