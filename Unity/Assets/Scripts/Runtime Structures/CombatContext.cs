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
        public Player CombatPlayer;

        public CampaignContext FromCampaign;
        public readonly Encounter BasedOnEncounter;
        public readonly List<Enemy> Enemies = new List<Enemy>();

        private readonly GameplayUXController UXController = null;
        private readonly Dictionary<string, List<ReactionWindowSubscription>> WindowsToReactors = new Dictionary<string, List<ReactionWindowSubscription>>();
        private readonly Dictionary<IReactionWindowReactor, HashSet<ReactionWindowSubscription>> ReactorsToSubscriptions = new Dictionary<IReactionWindowReactor, HashSet<ReactionWindowSubscription>>();

        public CombatContext(CampaignContext fromCampaign, Encounter basedOnEncounter, GameplayUXController uxController)
        {
            this.UXController = uxController;
            this.CombatPlayer = fromCampaign.CampaignPlayer;

            this.FromCampaign = fromCampaign;
            this.BasedOnEncounter = basedOnEncounter;
            this.PlayerCombatDeck = new CombatDeck(fromCampaign.CampaignDeck);
            this.PlayerCombatDeck.ShuffleEntireDeck();

            this.InitializeStartingEnemies();
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

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void EndCurrentTurnAndChangeTurn(TurnStatus toTurn)
        {
            // If an enemy doesn't have an intent, set it regardless of the phase we're moving towards.
            this.AssignEnemyIntents();

            if (this.CurrentTurnStatus == TurnStatus.PlayerTurn)
            {
                this.EndPlayerTurn(toTurn);
            }
            else if (this.CurrentTurnStatus == TurnStatus.EnemyTurn)
            {
                this.EndEnemyTurn(toTurn);
            }
            else
            {
                this.PlayerStartTurn();
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke();
            return;
        }

        /// <summary>
        /// Plays a specified card on the specified target.
        /// </summary>
        public void PlayCard(Card toPlay, ICombatantTarget toPlayOn)
        {
            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Card {toPlay.Name} can't be played, it isn't the player's turn.", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            if (!this.PlayerCombatDeck.CardsCurrentlyInHand.Contains(toPlay))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Card {toPlay.Name} appears to not be in hand. Has it already been played?", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            // Does the player meet the requirements of at least one of the effects?
            bool anyPassingRequirements = false;
            List<TokenEvaluatorBuilder> builders = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(toPlay);
            foreach (TokenEvaluatorBuilder builder in builders)
            {
                if (builder.MeetsElementRequirements(this))
                {
                    anyPassingRequirements = true;
                    break;
                }
            }

            if (!anyPassingRequirements)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Unable to play card {toPlay.Name}. No requirements for any of the card's effects have been met.", GlobalUpdateUX.LogType.GameEvent);
                UXController?.CancelAllSelections();
                return;
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"Playing card {toPlay.Name} on {toPlayOn.Name}", GlobalUpdateUX.LogType.GameEvent);
            UXController?.CancelAllSelections();
            this.PlayerCombatDeck.CardsCurrentlyInHand.Remove(toPlay);

            GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(
            () =>
            {
                GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this.FromCampaign, this.CombatPlayer, toPlay, toPlayOn);
                GlobalUpdateUX.LogTextEvent.Invoke(delta.DescribeDelta(), GlobalUpdateUX.LogType.GameEvent);
                delta.ApplyDelta(this.FromCampaign);
                this.CheckAllStateEffectsAndKnockouts();
            },
            () => UXController?.AnimateCardPlay(
                toPlay,
                toPlayOn)
            ));
        }

        public void EnemyAction(Enemy toAct, ICombatantTarget target)
        {
            if (toAct?.Intent == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Enemy {toAct.Name} has no Intent, cannot take action.", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this.FromCampaign, toAct, toAct.Intent, this.CombatPlayer);GlobalUpdateUX.LogTextEvent.Invoke($"The player has run out of health! This run is over.", GlobalUpdateUX.LogType.GameEvent);
            GlobalUpdateUX.LogTextEvent.Invoke(delta.DescribeDelta(), GlobalUpdateUX.LogType.GameEvent);
            delta.ApplyDelta(this.FromCampaign);

            this.CheckAllStateEffectsAndKnockouts();
        }

        public void StatusEffectHappeningProc(StatusEffectHappening happening)
        {
            GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this.FromCampaign, happening.OwnedStatusEffect.Owner, happening, this.CombatPlayer);
            GlobalUpdateUX.LogTextEvent.Invoke(delta.DescribeDelta(), GlobalUpdateUX.LogType.GameEvent);
            delta.ApplyDelta(this.FromCampaign);

            this.CheckAllStateEffectsAndKnockouts();
        }

        private void InitializeStartingEnemies()
        {
            foreach (EnemyModel curEnemyModel in this.BasedOnEncounter.GetEnemyModels())
            {
                Enemy enemyInstance = new Enemy(curEnemyModel);
                this.Enemies.Add(enemyInstance);
            }
        }

        void AssignEnemyIntents()
        {
            foreach (Enemy curEnemy in this.Enemies)
            {
                if (curEnemy.Intent != null)
                {
                    continue;
                }

                if (curEnemy.BaseModel.Attacks.Count == 0)
                {
                    continue;
                }

                int randomAttackIndex = UnityEngine.Random.Range(0, curEnemy.BaseModel.Attacks.Count);
                EnemyAttack randomAttack = curEnemy.BaseModel.Attacks[randomAttackIndex];
                curEnemy.Intent = randomAttack;

                List<ICombatantTarget> consideredTargets = new List<ICombatantTarget>()
                {
                    curEnemy,
                    this.CombatPlayer
                };

                List<ICombatantTarget> filteredTargets = ScriptTokenEvaluator.GetTargetsThatCanBeTargeted(curEnemy, curEnemy.Intent, consideredTargets);
                if (filteredTargets.Count == 0)
                {
                    randomAttack.PrecalculatedTarget = null;
                }
                else
                {
                    randomAttack.PrecalculatedTarget = filteredTargets[0];
                }
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void CheckAllStateEffectsAndKnockouts()
        {
            if (this.CombatPlayer.CurrentHealth <= 0)
            {
                this.PlayerDefeat();
                return;
            }

            List<Enemy> enemies = new List<Enemy>(this.Enemies);
            foreach (Enemy curEnemy in enemies)
            {
                if (curEnemy.ShouldBecomeDefeated && !curEnemy.DefeatHasBeenSignaled)
                {
                    this.RemoveEnemy(curEnemy);
                }
            }

            if (this.FromCampaign.CurrentNonCombatEncounterStatus == CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter && enemies.Count == 0)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"There are no more enemies!", GlobalUpdateUX.LogType.GameEvent);
                this.FromCampaign.SetCampaignState(CampaignContext.GameplayCampaignState.ClearedRoom);
                // this.SetupClearedRoomAndPresentAwards();
                return;
            }

            this.UXController?.UpdateUX();
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

        private void PlayerDefeat()
        {
            foreach (AppliedStatusEffect effect in CombatPlayer.AppliedStatusEffects)
            {
                this.UnsubscribeReactor(effect);
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"The player has run out of health! This run is over.", GlobalUpdateUX.LogType.GameEvent);
            this.FromCampaign.SetCampaignState(CampaignContext.GameplayCampaignState.Defeat);
            this.UnsubscribeReactor(this.CombatPlayer);

            GlobalSequenceEventHolder.StopAllSequences();
        }

        private void RemoveEnemy(Enemy toRemove)
        {
            toRemove.DefeatHasBeenSignaled = true;

            GlobalUpdateUX.LogTextEvent.Invoke($"{toRemove.Name} has been defeated!", GlobalUpdateUX.LogType.GameEvent);

            GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(
                () =>
                {
                    this.UXController?.RemoveEnemy(toRemove);
                    this.Enemies.Remove(toRemove);

                    foreach (AppliedStatusEffect effect in toRemove.AppliedStatusEffects)
                    {
                        this.UnsubscribeReactor(effect);
                    }

                    this.UnsubscribeReactor(toRemove);
                    this.CheckAllStateEffectsAndKnockouts();
                },
                null));
        }

        private void PlayerStartTurn()
        {
            this.CurrentTurnStatus = TurnStatus.PlayerTurn;

            this.CheckAndApplyReactionWindow(new ReactionWindowContext(KnownReactionWindows.OwnerStartsTurn, this.CombatPlayer));

            const int playerhandsize = 5;
            GlobalSequenceEventHolder.PushSequencesToTop(
                new GameplaySequenceEvent(
                    () => this.PlayerCombatDeck.DealCards(playerhandsize),
                    null)
                );
        }

        private void EndPlayerTurn(TurnStatus toTurn)
        {
            List<GameplaySequenceEvent> nextEvents = new List<GameplaySequenceEvent>();

            nextEvents.Add(new GameplaySequenceEvent(() => { this.CheckAndApplyReactionWindow(new ReactionWindowContext(KnownReactionWindows.OwnerEndsTurn, this.CombatPlayer)); }));
            nextEvents.Add(new GameplaySequenceEvent(() => { this.PlayerCombatDeck.DiscardHand(); }));

            if (toTurn == TurnStatus.EnemyTurn)
            {
                nextEvents.Add(new GameplaySequenceEvent(() => { this.EnemyStartTurn(); }));
            }

            GlobalSequenceEventHolder.PushSequencesToTop(nextEvents.ToArray());
        }

        private void EnemyStartTurn()
        {
            this.CurrentTurnStatus = TurnStatus.EnemyTurn;

            List<GameplaySequenceEvent> nextEvents = new List<GameplaySequenceEvent>();

            foreach (Enemy curEnemy in new List<Enemy>(this.Enemies))
            {
                if (this.TryGetReactionWindowSequenceEvents(new ReactionWindowContext(KnownReactionWindows.OwnerStartsTurn, curEnemy), out List<GameplaySequenceEvent> windowEvents))
                {
                    nextEvents.AddRange(windowEvents);
                }
            }

            foreach (Enemy curEnemy in new List<Enemy>(this.Enemies))
            {
                nextEvents.Add(
                    new GameplaySequenceEvent(() =>
                    {
                        this.EnemyAction(curEnemy, curEnemy?.Intent?.PrecalculatedTarget);
                    }));
            }
            
            nextEvents.Add(new GameplaySequenceEvent(() => this.EndEnemyTurn(TurnStatus.PlayerTurn)));

            GlobalSequenceEventHolder.PushSequencesToTop(nextEvents.ToArray());
        }

        private void EndEnemyTurn(TurnStatus toTurn)
        {
            List<GameplaySequenceEvent> nextEvents = new List<GameplaySequenceEvent>();

            foreach (Enemy curEnemy in this.Enemies)
            {
                nextEvents.Add(new GameplaySequenceEvent(() => { this.CheckAndApplyReactionWindow(new ReactionWindowContext(KnownReactionWindows.OwnerEndsTurn, curEnemy)); }));
            }

            if (toTurn == TurnStatus.PlayerTurn)
            {
                nextEvents.Add(new GameplaySequenceEvent(() => { this.PlayerStartTurn(); }));
            }

            GlobalSequenceEventHolder.PushSequencesToTop(nextEvents.ToArray());
        }
    }
}