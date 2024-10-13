namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
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

        public Dictionary<Element, int> ElementResourceCounts { get; private set; } = new Dictionary<Element, int>();
        public TurnStatus CurrentTurnStatus { get; private set; } = TurnStatus.NotInCombat;

        public readonly CombatDeck PlayerCombatDeck;
        public Player CombatPlayer;

        public CampaignContext FromCampaign;
        public readonly EvaluatedEncounter BasedOnEncounter;
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public Reward Rewards => this.BasedOnEncounter.Rewards;

        public CombatContext(CampaignContext fromCampaign, EvaluatedEncounter basedOnEncounter)
        {
            this.CombatPlayer = fromCampaign.CampaignPlayer;

            this.FromCampaign = fromCampaign;
            this.BasedOnEncounter = basedOnEncounter;
            this.PlayerCombatDeck = new CombatDeck(fromCampaign.CampaignDeck);
            this.PlayerCombatDeck.ShuffleEntireDeck();

            this.InitializeStartingEnemies();
        }

        public bool MeetsElementRequirement(Element element, int minimumCount)
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

        public void ApplyElementResourceChange(TokenEvaluatorBuilder fromBuilder, ElementResourceChange toChange)
        {
            if (toChange.GainOrLoss != null)
            {
                if (!toChange.GainOrLoss.TryEvaluateValue(this.FromCampaign, fromBuilder, out int evaluatedValue))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke($"Failed to parse evaluatable value for applying resource change.", GlobalUpdateUX.LogType.RuntimeError);
                    return;
                }

                if (this.ElementResourceCounts.TryGetValue(toChange.Element, out int currentAmount))
                {
                    int newAmount = Mathf.Max(0, currentAmount + evaluatedValue);

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
                    if (evaluatedValue > 0)
                    {
                        this.ElementResourceCounts.Add(toChange.Element, evaluatedValue);
                    }
                }
            }
            else if (toChange.SetValue != null)
            {
                if (!toChange.SetValue.TryEvaluateValue(this.FromCampaign, fromBuilder, out int evaluatedValue))
                {
                    GlobalUpdateUX.LogTextEvent?.Invoke($"Failed to parse evaluatable value for applying resource change.", GlobalUpdateUX.LogType.RuntimeError);
                    return;
                }

                if (this.ElementResourceCounts.ContainsKey(toChange.Element))
                {
                    int newAmount = Mathf.Max(0, evaluatedValue);

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
                    if (evaluatedValue > 0)
                    {
                        this.ElementResourceCounts.Add(toChange.Element, evaluatedValue);
                    }
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
            List<ConceptualTokenEvaluatorBuilder> conceptBuilders = ScriptTokenEvaluator.CalculateConceptualBuildersFromTokenEvaluation(toPlay);
            bool anyPassingRequirements = ScriptTokenEvaluator.MeetsAnyRequirements(conceptBuilders, this.FromCampaign, toPlay, this.CombatPlayer, toPlayOn);

            if (!anyPassingRequirements)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Playing {toPlay.Name} has not meant the requirements for any of its effects. The card will still be played to gain elements.", GlobalUpdateUX.LogType.GameEvent);
                // return;
            }

            GlobalUpdateUX.LogTextEvent.Invoke($"Playing card {toPlay.Name} on {toPlayOn.Name}", GlobalUpdateUX.LogType.GameEvent);

            // The card is not in any zone, temporarily. It will later be moved to discard.
            this.PlayerCombatDeck.CardsCurrentlyInHand.Remove(toPlay);

            GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(
            () =>
            {
                GamestateDelta delta = ScriptTokenEvaluator.CalculateRealizedDeltaEvaluation(toPlay, this.FromCampaign, this.CombatPlayer, toPlayOn);
                GlobalUpdateUX.LogTextEvent.Invoke(EffectDescriberDatabase.DescribeResolvedEffect(delta), GlobalUpdateUX.LogType.GameEvent);
                delta.ApplyDelta(this.FromCampaign);
                this.FromCampaign.CheckAllStateEffectsAndKnockouts();
                this.PlayerCombatDeck.MoveCardToZoneIfNotInAnyZonesCurrently(toPlay, this.PlayerCombatDeck.CardsCurrentlyInDiscard);

                GlobalUpdateUX.UpdateUXEvent.Invoke();
            },
            null
            ));
        }

        public void EnemyAction(Enemy toAct, ICombatantTarget target)
        {
            if (toAct?.Intent == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Enemy {toAct.Name} has no Intent, cannot take action.", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            GamestateDelta delta = ScriptTokenEvaluator.CalculateRealizedDeltaEvaluation(toAct.Intent, this.FromCampaign, toAct, this.CombatPlayer);
            GlobalUpdateUX.LogTextEvent.Invoke(EffectDescriberDatabase.DescribeResolvedEffect(delta), GlobalUpdateUX.LogType.GameEvent);
            delta.ApplyDelta(this.FromCampaign);

            toAct.Intent = null;

            this.FromCampaign.CheckAllStateEffectsAndKnockouts();
        }

        private void InitializeStartingEnemies()
        {
            foreach (Enemy curEnemy in this.BasedOnEncounter.Enemies)
            {
                this.Enemies.Add(curEnemy);
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

        public void RemoveEnemy(Enemy toRemove)
        {
            toRemove.DefeatHasBeenSignaled = true;

            GlobalUpdateUX.LogTextEvent.Invoke($"{toRemove.Name} has been defeated!", GlobalUpdateUX.LogType.GameEvent);

            GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(
                () =>
                {
                    this.Enemies.Remove(toRemove);

                    foreach (AppliedStatusEffect effect in toRemove.AppliedStatusEffects)
                    {
                        this.FromCampaign.UnsubscribeReactor(effect);
                    }

                    this.FromCampaign.UnsubscribeReactor(toRemove);
                    this.FromCampaign.CheckAllStateEffectsAndKnockouts();
                },
                null));
        }

        private void PlayerStartTurn()
        {
            this.ElementResourceCounts.Clear();
            this.CurrentTurnStatus = TurnStatus.PlayerTurn;

            this.FromCampaign.CheckAndApplyReactionWindow(new ReactionWindowContext(this.FromCampaign, KnownReactionWindows.OwnerStartsTurn, this.CombatPlayer));

            const int playerhandsize = 5;
            GlobalSequenceEventHolder.PushSequencesToTop(
                this.FromCampaign,
                new GameplaySequenceEvent(
                    () => this.PlayerCombatDeck.DealCards(playerhandsize),
                    null)
                );
        }

        private void EndPlayerTurn(TurnStatus toTurn)
        {
            List<GameplaySequenceEvent> nextEvents = new List<GameplaySequenceEvent>();

            nextEvents.Add(new GameplaySequenceEvent(() => { this.FromCampaign.CheckAndApplyReactionWindow(new ReactionWindowContext(
                this.FromCampaign,
                KnownReactionWindows.OwnerEndsTurn, 
                this.CombatPlayer)); }));
            nextEvents.Add(new GameplaySequenceEvent(() => { this.PlayerCombatDeck.DiscardHand(); }));

            if (toTurn == TurnStatus.EnemyTurn)
            {
                nextEvents.Add(new GameplaySequenceEvent(() => { this.EnemyStartTurn(); }));
            }

            GlobalSequenceEventHolder.PushSequencesToTop(this.FromCampaign, nextEvents.ToArray());
        }

        private void EnemyStartTurn()
        {
            this.CurrentTurnStatus = TurnStatus.EnemyTurn;

            List<GameplaySequenceEvent> nextEvents = new List<GameplaySequenceEvent>();

            foreach (Enemy curEnemy in new List<Enemy>(this.Enemies))
            {
                if (this.FromCampaign.TryGetReactionWindowSequenceEvents(new ReactionWindowContext(this.FromCampaign, KnownReactionWindows.OwnerStartsTurn, curEnemy), out List<GameplaySequenceEvent> windowEvents))
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
            
            nextEvents.Add(new GameplaySequenceEvent(() => this.EndCurrentTurnAndChangeTurn(TurnStatus.PlayerTurn)));

            GlobalSequenceEventHolder.PushSequencesToTop(this.FromCampaign, nextEvents.ToArray());
        }

        private void EndEnemyTurn(TurnStatus toTurn)
        {
            List<GameplaySequenceEvent> nextEvents = new List<GameplaySequenceEvent>();

            foreach (Enemy curEnemy in this.Enemies)
            {
                nextEvents.Add(new GameplaySequenceEvent(() => { this.FromCampaign.CheckAndApplyReactionWindow(new ReactionWindowContext(this.FromCampaign, KnownReactionWindows.OwnerEndsTurn, curEnemy)); }));
            }

            if (toTurn == TurnStatus.PlayerTurn)
            {
                nextEvents.Add(new GameplaySequenceEvent(() => { this.PlayerStartTurn(); }));
            }

            GlobalSequenceEventHolder.PushSequencesToTop(this.FromCampaign, nextEvents.ToArray());
        }
    }
}