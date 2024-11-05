namespace SFDDCards
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
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
            AllowedToLeave = 1,
            NotAllowedToLeave = 2
        }

        public readonly Deck CampaignDeck;
        public CombatContext CurrentCombatContext { get; private set; } = null;
        public EvaluatedEncounter CurrentEncounter { get; private set; } = null;
        public readonly Player CampaignPlayer;
        public CampaignRoute OnRoute { get; private set; } = null;
        public int CampaignRouteNodeIndex { get; private set; } = -1;

        public GameplayCampaignState CurrentGameplayCampaignState { get; private set; } = GameplayCampaignState.NotStarted;
        public NonCombatEncounterStatus CurrentNonCombatEncounterStatus { get; private set; } = NonCombatEncounterStatus.NotInNonCombatEncounter;

        private readonly Dictionary<IReactionWindowReactor, HashSet<ReactionWindowSubscription>> ReactorsToSubscriptions = new Dictionary<IReactionWindowReactor, HashSet<ReactionWindowSubscription>>();
        private readonly Dictionary<string, List<ReactionWindowSubscription>> WindowsToReactors = new Dictionary<string, List<ReactionWindowSubscription>>();

        public Dictionary<CurrencyImport, int> CurrencyCounts = new Dictionary<CurrencyImport, int>();
        public Reward PendingRewards { get; set; } = null;

        public CampaignContext(CampaignRoute onRoute)
        {
            this.CampaignPlayer = new Player(onRoute.BasedOn.StartingMaximumHealth);
            this.OnRoute = onRoute;

            this.CampaignDeck = new Deck(this);
            foreach (string startingCard in onRoute.BasedOn.StartingDeck)
            {
                this.CampaignDeck.AddCardToDeck(CardDatabase.GetModel(startingCard));
            }
        }

        public void AddCardToDeck(Card toAdd)
        {
            this.CampaignDeck.AddCardToDeck(toAdd);
        }

        public void LeaveCurrentEncounter()
        {
            if (this.CurrentCombatContext != null && this.CurrentCombatContext.BasedOnEncounter != null)
            {
                this.PendingRewards = this.CurrentCombatContext.Rewards;
            }

            this.CurrentCombatContext = null;
            this.CurrentEncounter = null;

            GlobalUpdateUX.UpdateUXEvent.Invoke(this);
        }

        public void StartNextRoomFromEncounter(EvaluatedEncounter basedOn)
        {
            this.LeaveCurrentEncounter();
            this.CurrentEncounter = basedOn;

            if (basedOn.BasedOn.IsShopEncounter)
            {
                this.SetCampaignState(GameplayCampaignState.NonCombatEncounter, NonCombatEncounterStatus.AllowedToLeave);
                return;
            }
            else if (basedOn.BasedOn.EncounterScripts != null && basedOn.BasedOn.EncounterScripts.Count > 0)
            {
                this.SetCampaignState(GameplayCampaignState.NonCombatEncounter, NonCombatEncounterStatus.NotAllowedToLeave);
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
                this.LeaveCurrentEncounter();
            }

            if (toState == GameplayCampaignState.MakingRouteChoice)
            {
                this.CampaignRouteNodeIndex++;

                if (this.OnRoute != null && this.CampaignRouteNodeIndex >= this.OnRoute.Nodes.Count)
                {
                    this.SetCampaignState(GameplayCampaignState.Victory);
                }
            }

            GlobalUpdateUX.UpdateUXEvent?.Invoke(this);
        }

        public void MakeChoiceNodeDecision(ChoiceNodeOption chosen)
        {
            chosen.WasSelected = true;
            this.CurrentEncounter = chosen.WillEncounter;
            this.StartNextRoomFromEncounter(chosen.WillEncounter);
            GlobalUpdateUX.UpdateUXEvent?.Invoke(this);
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

            GlobalSequenceEventHolder.PushSequencesToTop(this, eventsThatWouldFollow.ToArray());
        }

        public bool TryGetReactionWindowSequenceEvents(ReactionWindowContext context, out List<GameplaySequenceEvent> eventsThatWouldFollow)
        {
            eventsThatWouldFollow = null;

            if (this.WindowsToReactors.TryGetValue(context.TimingWindowId, out List<ReactionWindowSubscription> reactors))
            {
                foreach (ReactionWindowSubscription reactor in reactors)
                {
                    if (reactor != null && reactor.ShouldApply(context) && reactor.Reactor.TryGetReactionEvents(this, context, out List<WindowResponse> responses))
                    {
                        if (eventsThatWouldFollow == null)
                        {
                            eventsThatWouldFollow = new List<GameplaySequenceEvent>();
                        }

                        foreach (WindowResponse response in responses)
                        {
                            eventsThatWouldFollow.Add(new GameplaySequenceEvent(() => this.StatusEffectHappeningProc(new StatusEffectHappening(response))));
                        }
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
            ICombatantTarget originalTarget = happening.Context.CombatantTarget;

            GamestateDelta delta = ScriptTokenEvaluator.CalculateRealizedDeltaEvaluation(happening, this, happening.OwnedStatusEffect?.Owner, originalTarget, happening.Context);
            GlobalUpdateUX.LogTextEvent.Invoke(EffectDescriberDatabase.DescribeResolvedEffect(delta), GlobalUpdateUX.LogType.GameEvent);
            delta.ApplyDelta(this);

            this.CheckAllStateEffectsAndKnockouts();
        }

        public void IngestStatusEffectHappening(ReactionWindowContext reactionWindow, WindowResponse response)
        {
            GlobalSequenceEventHolder.PushSequencesToTop(reactionWindow.CampaignContext, response);
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

            GlobalUpdateUX.UpdateUXEvent?.Invoke(this);
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

        public int GetCurrencyCount(CurrencyImport toGet)
        {
            if (this.CurrencyCounts.TryGetValue(toGet, out int value))
            {
                return value;
            }

            return 0;
        }

        public void ModCurrency(CurrencyImport toAward, int amount)
        {
            if (this.CurrencyCounts.TryGetValue(toAward, out int existingValue))
            {
                this.CurrencyCounts[toAward] = Mathf.Max(0, existingValue + amount);
            }
            else
            {
                this.CurrencyCounts.Add(toAward, Mathf.Max(0, amount));
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke(this);
        }

        public void SetCurrency(CurrencyImport toSet, int amount)
        {
            if (this.CurrencyCounts.TryGetValue(toSet, out int existingValue))
            {
                this.CurrencyCounts[toSet] = Mathf.Max(0, amount);
            }
            else
            {
                this.CurrencyCounts.Add(toSet, Mathf.Max(0, amount));
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke(this);
        }

        public void PurchaseShopItem(ShopEntry toBuy)
        {
            bool canAfford = this.CanAfford(toBuy.Costs);

            if (!canAfford)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Cannot afford the cost of this item, or could not evaluate all its costs.", GlobalUpdateUX.LogType.UserError);
                return;
            }

            this.Gain(toBuy);

            foreach (ShopCost cost in toBuy.Costs)
            {
                if (!cost.Amount.TryEvaluateValue(this, null, out int shopCostAmount))
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"The cost of a shop item could not be evaluated.", GlobalUpdateUX.LogType.RuntimeError);
                }

                this.ModCurrency(cost.Currency, -shopCostAmount);
            }
        }

        public bool CanAfford(List<ShopCost> costs)
        {
            foreach (ShopCost cost in costs)
            {
                if (!cost.Amount.TryEvaluateValue(this, null, out int costAmount))
                {
                    return false;
                }

                int amountInPossession = GetCurrencyCount(cost.Currency);

                if (amountInPossession < costAmount)
                {
                    return false;
                }
            }

            return true;
        }

        public void Gain(IGainable toGain)
        {
            if (!toGain.GainedAmount.TryEvaluateValue(this, null, out int gainedAmount))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate gain amount for gainable. Could not gain.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            if (toGain.GainedCard != null)
            {
                for (int ii = 0; ii < gainedAmount; ii++)
                {
                    this.CampaignDeck.AddCardToDeck(toGain.GainedCard);
                }
            }
            else if (toGain.GainedEffect != null)
            {
                GlobalSequenceEventHolder.PushSequenceToTop(new GameplaySequenceEvent(() =>
                {
                    this.CampaignPlayer.ApplyDelta(
                        this,
                        null,
                        ScriptTokenEvaluator.GetDeltaFromTokens($"[SETTARGET:SELF][APPLYSTATUSEFFECTSTACKS: {gainedAmount} {toGain.GainedEffect.Id}]",
                        this,
                        null,
                        this.CampaignPlayer,
                        this.CampaignPlayer)
                        .DeltaEntries[0]);

                    GlobalUpdateUX.UpdateUXEvent?.Invoke(this);
                }));
            }
            else if (toGain.GainedCurrency != null)
            {
                this.ModCurrency(toGain.GainedCurrency, gainedAmount);
            }

            GlobalUpdateUX.UpdateUXEvent.Invoke(this);
        }

        public List<ShopCost> GetPriceForItem(IGainable toShopFor)
        {
            Dictionary<CurrencyImport, IEvaluatableValue<int>> costs = new Dictionary<CurrencyImport, IEvaluatableValue<int>>();

            foreach (CostEvaluationModifier modifier in this.OnRoute.BasedOn.CostModifiers)
            {
                if (!GainableMatchesTags(toShopFor, modifier.TagMatch))
                {
                    continue;
                }

                CurrencyImport referencedCurrency = CurrencyDatabase.GetModel(modifier.Currency);
                int valueToReplace = 0;

                if (costs.TryGetValue(referencedCurrency, out IEvaluatableValue<int> currentValue))
                {
                    if (!currentValue.TryEvaluateValue(this, null, out valueToReplace))
                    {
                        GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate existing cost from earlier rule.", GlobalUpdateUX.LogType.RuntimeError);
                        continue;
                    }
                }

                string replacedValueString = modifier.EvaluationScript.ToLower().Replace("value", valueToReplace.ToString());
                if (!BaseScriptingToken.TryGetIntegerEvaluatableFromString(replacedValueString, out IEvaluatableValue<int> newValue))
                {
                    GlobalUpdateUX.LogTextEvent.Invoke($"Failed to evaluate new rule.", GlobalUpdateUX.LogType.RuntimeError);
                    continue;
                }

                if (costs.ContainsKey(referencedCurrency))
                {
                    costs[referencedCurrency] = newValue;
                }
                else
                {
                    costs.Add(referencedCurrency, newValue);
                }
            }

            List<ShopCost> costsAsShopCost = new List<ShopCost>();
            foreach (CurrencyImport key in costs.Keys)
            {
                costsAsShopCost.Add(new ShopCost() { Amount = costs[key], Currency = key });
            }

            return costsAsShopCost;
        }

        public bool GainableMatchesTags(IGainable toMatch, IEnumerable<string> tags)
        {
            if (toMatch.GainedCard != null)
            {
                return toMatch.GainedCard.BasedOn.MeetsAllTags(tags);
            }
            else if (toMatch.GainedEffect != null)
            {
                return toMatch.GainedEffect.MeetsAllTags(tags);
            }
            else if (toMatch.GainedCurrency != null)
            {
                // TODO: Currency tags?
                return true;
            }

            return false;
        }

        public bool RequirementsAreMet(string requirementScript)
        {
            if (string.IsNullOrEmpty(requirementScript))
            {
                return true;
            }

            RequiresComparisonScriptingToken requiresComparisonBaseToken = new RequiresComparisonScriptingToken();
            if (requiresComparisonBaseToken.GetTokenIfMatch(requirementScript, out IScriptingToken match) && match is RequiresComparisonScriptingToken requiresComparison)
            {
                return requiresComparison.MeetsRequirement(null, this);
            }

            return true;
        }
    }
}