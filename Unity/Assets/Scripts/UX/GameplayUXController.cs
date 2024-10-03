namespace SFDDCards.UX
{
    using SFDDCards.Evaluation.Actual;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class GameplayUXController : MonoBehaviour
    {
        [SerializeReference]
        private CentralGameStateController CentralGameStateControllerInstance;
        [SerializeReference]
        private AnimationRunnerController AnimationRunnerController;
        [SerializeReference]
        private PlayerStatusEffectUXHolder PlayerStatusEffectUXHolderInstance;
        [SerializeReference]
        private CombatTurnController CombatTurnCounterInstance;
        [SerializeReference]
        private CampaignChooserUX CampaignChooserUXInstance;
        [SerializeReference]
        private CardBrowser CardBrowserUXInstance;

        [SerializeReference]
        private RewardsPanelUX RewardsPanelUXInstance;
        [SerializeReference]
        private ShopUX ShopPanelUXInstance;

        [SerializeReference]
        private PlayerUX PlayerRepresentationPF;
        private PlayerUX PlayerUXInstance { get; set; }

        [SerializeReference]
        private EnemyUX EnemyRepresentationPF;
        [SerializeReference]
        private Transform PlayerRepresentationTransform;
        [SerializeReference]
        private Transform EnemyRepresntationTransform;

        [SerializeReference]
        private PlayerHandRepresenter PlayerHandRepresenter;

        [SerializeReference]
        private GameObject GoNextRoomButton;
        [SerializeReference]
        private GameObject ResetGameButton;
        [SerializeReference]
        private GameObject EndTurnButton;

        [SerializeReference]
        private ChoiceNodeSelectorUX ChoiceSelectorUX;
        [SerializeReference]
        private GameObject CombatUXFolder;
        [SerializeReference]
        private GameObject ChoiceUXFolder;

        [SerializeReference]
        private TMPro.TMP_Text LifeValue;
        [SerializeReference]
        private TMPro.TMP_Text ElementsValue;

        [SerializeReference]
        private TargetableIndicator SingleCombatantTargetableIndicatorPF;
        private List<TargetableIndicator> ActiveIndicators { get; set; } = new List<TargetableIndicator>();
        [SerializeReference]
        private TargetableIndicator NoTargetsIndicator;
        [SerializeReference]
        private TargetableIndicator AllFoeTargetsIndicator;

        [SerializeReference]
        private GameObject AllCardsBrowserButton;

        [SerializeReference]
        private TMPro.TMP_Text Log;

        private Dictionary<Enemy, EnemyUX> spawnedEnemiesLookup { get; set; } = new Dictionary<Enemy, EnemyUX>();

        public DisplayedCardUX CurrentSelectedCard { get; private set; } = null;
        public bool PlayerIsCurrentlyAnimating { get; private set; } = false;

        private CampaignContext.GameplayCampaignState previousCampaignState { get; set; } = CampaignContext.GameplayCampaignState.NotStarted;
        private CombatContext.TurnStatus previousCombatTurnState { get; set; } = CombatContext.TurnStatus.NotInCombat;
        private CampaignContext.NonCombatEncounterStatus previousNonCombatEncounterState { get; set; } = CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter;

        public CampaignContext CurrentCampaignContext => this.CentralGameStateControllerInstance?.CurrentCampaignContext;

        private void Awake()
        {
            this.Annihilate();

            GlobalUpdateUX.LogTextEvent.AddListener(this.AddToLog);
        }

        private void OnEnable()
        {
            GlobalUpdateUX.UpdateUXEvent.AddListener(UpdateUX);
        }

        private void OnDestroy()
        {
            GlobalUpdateUX.UpdateUXEvent.RemoveListener(UpdateUX);
            GlobalUpdateUX.LogTextEvent.RemoveListener(AddToLog);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(1))
            {
                this.CancelAllSelections();
            }
        }

        public void PlacePlayerCharacter()
        {
            if (this.PlayerUXInstance != null)
            {
                Destroy(this.PlayerUXInstance.gameObject);
                this.PlayerUXInstance = null;
            }

            this.PlayerUXInstance = Instantiate(this.PlayerRepresentationPF, this.PlayerRepresentationTransform);
            this.PlayerUXInstance.SetFromPlayer(this.CurrentCampaignContext.CampaignPlayer);

            this.LifeValue.text = $"{this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer.CurrentHealth} / {this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer.MaxHealth}";
        }

        public void CheckAndActIfGameCampaignNavigationStateChanged()
        {
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext == null)
            {
                this.GoNextRoomButton.SetActive(false);
                this.EndTurnButton.SetActive(false);
                MouseHoverShowerPanel.CurrentContext = null;
                return;
            }

            this.CampaignChooserUXInstance.HideChooser();

            CampaignContext.GameplayCampaignState newCampaignState = this.CurrentCampaignContext.CurrentGameplayCampaignState;
            CampaignContext.NonCombatEncounterStatus newNonCombatState = this.CurrentCampaignContext.CurrentNonCombatEncounterStatus;
            CombatContext.TurnStatus newTurnState = this.CurrentCampaignContext.CurrentCombatContext == null ? CombatContext.TurnStatus.NotInCombat : this.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus;

            CampaignContext.GameplayCampaignState wasPreviousCampaignState = this.previousCampaignState;
            CampaignContext.NonCombatEncounterStatus wasPreviousNonCombatState = this.previousNonCombatEncounterState;
            CombatContext.TurnStatus wasPreviousTurnState = this.previousCombatTurnState;

            if (wasPreviousCampaignState == newCampaignState
                && wasPreviousNonCombatState == newNonCombatState
                && wasPreviousTurnState == newTurnState)
            {
                return;
            }

            this.previousCampaignState = newCampaignState;
            this.previousNonCombatEncounterState = newNonCombatState;
            this.previousCombatTurnState = newTurnState;

            if (newCampaignState == CampaignContext.GameplayCampaignState.ClearedRoom
                || (newCampaignState == CampaignContext.GameplayCampaignState.NonCombatEncounter && newNonCombatState == CampaignContext.NonCombatEncounterStatus.AllowedToLeave))
            {
                this.GoNextRoomButton.SetActive(true);

                if (wasPreviousCampaignState == CampaignContext.GameplayCampaignState.InCombat 
                    && this.CurrentCampaignContext?.PendingRewards != null)
                {
                    this.PresentAwards(this.CurrentCampaignContext.PendingRewards);
                }
            }
            else
            {
                this.RewardsPanelUXInstance.gameObject.SetActive(false);
                this.ShopPanelUXInstance.gameObject.SetActive(false);
                this.GoNextRoomButton.SetActive(false);
            }

            if (newCampaignState == CampaignContext.GameplayCampaignState.NonCombatEncounter)
            {
                if (wasPreviousCampaignState != CampaignContext.GameplayCampaignState.NonCombatEncounter &&
                    this.CentralGameStateControllerInstance.CurrentCampaignContext != null &&
                    this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentEncounter != null &&
                    this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentEncounter.BasedOn.IsShopEncounter)
                {
                    this.ShowShopPanel(this.CurrentCampaignContext.CurrentEncounter.GetCards().ToArray());
                }
            }
            
            if (newCampaignState == CampaignContext.GameplayCampaignState.InCombat)
            {
                this.CombatUXFolder.SetActive(true);
                this.RewardsPanelUXInstance.gameObject.SetActive(false);
                this.ShopPanelUXInstance.gameObject.SetActive(false);

                if (wasPreviousCampaignState != CampaignContext.GameplayCampaignState.InCombat)
                {
                    this.CombatTurnCounterInstance.BeginHandlingCombat();
                }

                if (newTurnState == CombatContext.TurnStatus.PlayerTurn)
                {
                    this.EndTurnButton.SetActive(true);
                }
                else
                {
                    this.EndTurnButton.SetActive(false);
                }

                MouseHoverShowerPanel.CurrentContext = new ReactionWindowContext(this.CentralGameStateControllerInstance.CurrentCampaignContext, KnownReactionWindows.ConsideringPlayingFromHand, this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.CombatPlayer, combatantTarget: null, playedFromZone: "hand");
            }
            else
            {
                if (wasPreviousCampaignState == CampaignContext.GameplayCampaignState.InCombat)
                {
                    this.CombatTurnCounterInstance.EndHandlingCombat();
                }

                this.EndTurnButton.SetActive(false);
                this.CombatUXFolder.SetActive(false);
            }

            if (newCampaignState == CampaignContext.GameplayCampaignState.MakingRouteChoice)
            {
                this.PresentNextRouteChoice();
            }
            else
            {
                this.ClearRouteUX();
            }

            if (newCampaignState == CampaignContext.GameplayCampaignState.Victory)
            {
                GlobalUpdateUX.LogTextEvent?.Invoke($"Victory!! There are no more nodes in this route! Reset game to continue from beginning.", GlobalUpdateUX.LogType.GameEvent);
            }
        }

        public void UpdateUX()
        {
            if (this.CardBrowserUXInstance.gameObject.activeInHierarchy)
            {
                this.AllCardsBrowserButton.SetActive(false);
            }
            else
            {
                this.AllCardsBrowserButton.SetActive(true);
            }

            this.CheckAndActIfGameCampaignNavigationStateChanged();
            this.RemoveDefeatedEntities();
            this.SetElementValueLabel();
            this.UpdateEnemyUX();
            this.UpdatePlayerLabelValues();
            this.RepresentTargetables();
        }

        public void SelectTarget(ICombatantTarget toSelect)
        {
            if (this.CurrentSelectedCard == null || this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null)
            {
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
            {
                return;
            }

            this.CombatTurnCounterInstance.PlayCard(this.CurrentSelectedCard.RepresentedCard, toSelect);
            this.CurrentSelectedCard = null;
        }

        public void AddEnemy(Enemy toAdd)
        {
            Vector3 objectOffset = new Vector3(3f, 0, 0) * this.EnemyRepresntationTransform.childCount;
            EnemyUX newEnemy = Instantiate(this.EnemyRepresentationPF, this.EnemyRepresntationTransform);
            newEnemy.transform.localPosition = objectOffset;
            newEnemy.SetFromEnemy(toAdd, this.CentralGameStateControllerInstance);
            this.spawnedEnemiesLookup.Add(toAdd, newEnemy);
        }

        public void RemoveEnemy(Enemy toRemove)
        {
            if (this.spawnedEnemiesLookup.TryGetValue(toRemove, out EnemyUX ux))
            {
                EnemyUX representation = this.spawnedEnemiesLookup[toRemove];
                Destroy(representation.gameObject);
                this.spawnedEnemiesLookup.Remove(toRemove);
            }
        }

        public void SelectCurrentCard(DisplayedCardUX toSelect)
        {
            if (this.CurrentCampaignContext.CurrentCombatContext == null ||
                this.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
            {
                return;
            }

            if (this.PlayerIsCurrentlyAnimating)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Player is currently animating, please wait until finished.", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            if (this.CurrentSelectedCard != null)
            {
                this.CurrentSelectedCard.DisableSelectionGlow();
                this.CurrentSelectedCard = null;
            }

            this.CurrentSelectedCard = toSelect;
            this.CurrentSelectedCard.EnableSelectionGlow();
            this.AppointTargetableIndicatorsToValidTargets(toSelect.RepresentedCard);
            GlobalUpdateUX.LogTextEvent.Invoke($"Selected card {toSelect.RepresentedCard.Name}", GlobalUpdateUX.LogType.GameEvent);
        }

        public void AddToLog(string text, GlobalUpdateUX.LogType logType)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (!text.EndsWith("\n"))
            {
                text += "\n";
            }
            this.Log.text += text;

            const int maximumLogSize = 10000;
            if (this.Log.text.Length > maximumLogSize)
            {
                this.Log.text = this.Log.text.Substring(this.Log.text.Length - maximumLogSize, maximumLogSize);
            }

            Debug.Log(text);
        }

        public void CancelAllSelections()
        {
            this.CurrentSelectedCard?.DisableSelectionGlow();
            this.CurrentSelectedCard = null;

            this.CardBrowserUXInstance.Close();

            ClearAllTargetableIndicators();
        }

        public void AnimateEnemyTurns(Action continuationAction)
        {
            this.StartCoroutine(AnimateEnemyTurnsInternal(continuationAction));
        }

        public void ShowRewardsPanel(Reward cardsToReward)
        {
            this.RewardsPanelUXInstance.gameObject.SetActive(true);
            this.RewardsPanelUXInstance.SetReward(cardsToReward);
            this.UpdateUX();
        }

        public void ShowShopPanel(params Card[] cardsInShop)
        {
            this.ShopPanelUXInstance.gameObject.SetActive(true);
            this.ShopPanelUXInstance.SetRewardCards(cardsInShop);
        }

        private IEnumerator AnimateEnemyTurnsInternal(Action continuationAction)
        {
            foreach (Enemy curEnemy in this.CurrentCampaignContext.CurrentCombatContext.Enemies)
            {
                yield return AnimateAction(this.spawnedEnemiesLookup[curEnemy], curEnemy.Intent, curEnemy.Intent.PrecalculatedTarget);
            }

            continuationAction.Invoke();
        }

        public IEnumerator AnimateCardPlay(Card toPlay, ICombatantTarget target)
        {
            yield return AnimateCardPlayInternal(toPlay, target);
        }

        private IEnumerator AnimateCardPlayInternal(Card toPlay, ICombatantTarget target)
        {
            PlayerIsCurrentlyAnimating = true;

            yield return AnimateAction(this.PlayerUXInstance, toPlay, target);

            PlayerIsCurrentlyAnimating = false;
        }

        private IEnumerator AnimateAction(IAnimationPuppet puppet, IAttackTokenHolder attack, ICombatantTarget target)
        {
            IAnimationPuppet targetPuppet = null;

            if (target is Player)
            {
                targetPuppet = this.PlayerUXInstance;
            }
            else if (target is Enemy targetEnemy)
            {
                targetPuppet = this.spawnedEnemiesLookup[targetEnemy];
            }

            if (targetPuppet == null || targetPuppet == puppet)
            {
                yield return this.AnimationRunnerController.AnimateUpwardNod(
                    puppet
                );
            }
            else
            {
                yield return this.AnimationRunnerController.AnimateShoveAttack(
                    puppet,
                    targetPuppet
                );
            }
        }

        void UpdatePlayerLabelValues()
        {
            if (this.CurrentCampaignContext?.CampaignPlayer == null)
            {
                this.LifeValue.text = "0";
                this.PlayerStatusEffectUXHolderInstance.Annihilate();

                return;
            }

            this.LifeValue.text = this.CurrentCampaignContext.CampaignPlayer.CurrentHealth.ToString();
            this.PlayerStatusEffectUXHolderInstance.SetStatusEffects(
                this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer.AppliedStatusEffects,
                this.StatusEffectClicked);
        }

        private void SetElementValueLabel()
        {
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext == null ||
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null ||
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.ElementResourceCounts == null || this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.ElementResourceCounts.Count == 0)
            {
                this.ElementsValue.text = "None";
                return;
            }

            string startingSeparator = "";
            StringBuilder compositeElements = new StringBuilder();
            foreach (Element element in this.CurrentCampaignContext.CurrentCombatContext.ElementResourceCounts.Keys)
            {
                compositeElements.Append($"{startingSeparator}{this.CurrentCampaignContext.CurrentCombatContext.ElementResourceCounts[element]}\u00A0{element.GetNameOrIcon()}");
                startingSeparator = ", ";
            }

            this.ElementsValue.text = compositeElements.ToString();
        }

        private void ClearAllTargetableIndicators()
        {
            if (this.ActiveIndicators != null)
            {
                for (int ii = 0; ii < this.ActiveIndicators.Count; ii++)
                {
                    Destroy(this.ActiveIndicators[ii].gameObject);
                }

                this.ActiveIndicators.Clear();
            }

            this.NoTargetsIndicator.gameObject.SetActive(false);
            this.AllFoeTargetsIndicator.gameObject.SetActive(false);
        }

        private void AppointTargetableIndicatorsToValidTargets(Card toTarget)
        {
            this.ClearAllTargetableIndicators();

            List<ICombatantTarget> possibleTargets = new List<ICombatantTarget>();
            possibleTargets.Add(this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer);

            foreach (Enemy curEnemy in this.spawnedEnemiesLookup.Keys)
            {
                possibleTargets.Add(curEnemy);
            }

            AllFoesTarget allFoesTarget = new AllFoesTarget(new List<ICombatantTarget>(this.spawnedEnemiesLookup.Keys));
            possibleTargets.Add(allFoesTarget);

            List<ICombatantTarget> remainingTargets = ScriptTokenEvaluator.GetTargetsThatCanBeTargeted(this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer, toTarget, possibleTargets);

            if (remainingTargets.Count > 0)
            {
                if (remainingTargets[0] is AllFoesTarget)
                {
                    this.AllFoeTargetsIndicator.SetFromTarget(allFoesTarget, SelectTarget);
                    this.AllFoeTargetsIndicator.gameObject.SetActive(true);
                }
                else
                {
                    foreach (ICombatantTarget target in remainingTargets)
                    {
                        TargetableIndicator indicator = Instantiate(this.SingleCombatantTargetableIndicatorPF, target.UXPositionalTransform);
                        indicator.SetFromTarget(target, this.SelectTarget);
                        this.ActiveIndicators.Add(indicator);
                    }
                }
            }
            else
            {
                this.NoTargetsIndicator.SetFromTarget(new NoTarget(), SelectTarget);
                this.NoTargetsIndicator.gameObject.SetActive(true);
            }
        }

        void UpdateEnemyUX()
        {
            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext?.Enemies == null || this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext?.Enemies?.Count == 0)
            {
                return;
            }

            foreach (Enemy curEnemy in this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext?.Enemies)
            {
                if (!this.spawnedEnemiesLookup.TryGetValue(curEnemy, out EnemyUX value))
                {
                    // It could be that the UpdateUX call was made before these enemies are spawned in to the game
                    // In that case, just continue
                    continue;
                }

                value.UpdateUX(this.CentralGameStateControllerInstance);
            }
        }

        public void Annihilate()
        {
            this.PlayerHandRepresenter.Annihilate();

            for (int ii = this.EnemyRepresntationTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.EnemyRepresntationTransform.GetChild(ii).gameObject);
            }

            for (int ii = this.PlayerRepresentationTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerRepresentationTransform.GetChild(ii).gameObject);
            }

            if (this.PlayerUXInstance != null)
            {
                Destroy(this.PlayerUXInstance.gameObject);
            }

            this.ShopPanelUXInstance.gameObject.SetActive(false);
            this.RewardsPanelUXInstance.gameObject.SetActive(false);
            this.PlayerStatusEffectUXHolderInstance.Annihilate();

            this.UpdateUX();
        }

        void RepresentTargetables()
        {
            if (this.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                ClearAllTargetableIndicators();
                return;
            }

            if (this.CurrentCampaignContext.CurrentGameplayCampaignState != CampaignContext.GameplayCampaignState.InCombat)
            {
                ClearAllTargetableIndicators();
                return;
            }

            if (this.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
            {
                ClearAllTargetableIndicators();
                return;
            }

            if (this.CurrentSelectedCard == null)
            {
                ClearAllTargetableIndicators();
                return;
            }

            this.AppointTargetableIndicatorsToValidTargets(this.CurrentSelectedCard.RepresentedCard);
        }

        public void EndTurn()
        {
            this.CancelAllSelections();

            if (GlobalSequenceEventHolder.StackedSequenceEvents.Count > 0)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Animations and events are happening, can't end turn yet.", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"It's not the player's turn, can't end turn.", GlobalUpdateUX.LogType.GameEvent);
                return;
            }

            this.CombatTurnCounterInstance.EndPlayerTurn();
        }

        public void PresentAwards(Reward toPresent)
        {
            this.CancelAllSelections();
            this.ShowRewardsPanel(toPresent);
        }

        void PresentNextRouteChoice()
        {
            this.CancelAllSelections();

            ChoiceNode campaignNode = this.CurrentCampaignContext.GetCampaignCurrentNode();

            if (campaignNode == null)
            {
                GlobalUpdateUX.LogTextEvent?.Invoke($"The next campaign node is null. Cannot continue with UX.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            this.ChoiceUXFolder.SetActive(true);
            this.ChoiceSelectorUX.RepresentNode(campaignNode);
        }

        void ClearRouteUX()
        {
            this.CancelAllSelections();
            this.ChoiceUXFolder.SetActive(false);
        }

        public void NodeIsChosen(ChoiceNodeOption option)
        {
            this.CancelAllSelections();
            this.CentralGameStateControllerInstance.CurrentCampaignContext.MakeChoiceNodeDecision(option);
        }

        public void ProceedToNextRoom()
        {
            this.CancelAllSelections();
            this.CentralGameStateControllerInstance.CurrentCampaignContext.SetCampaignState(CampaignContext.GameplayCampaignState.MakingRouteChoice);
        }

        public void RouteChosen(RouteImport chosenRoute)
        {
            this.CancelAllSelections();
            this.CentralGameStateControllerInstance.RouteChosen(chosenRoute);
        }

        public void ShowCampaignChooser()
        {
            this.CampaignChooserUXInstance.ShowChooser();
        }

        private void RemoveDefeatedEntities()
        {
            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                foreach (Enemy key in new List<Enemy>(this.spawnedEnemiesLookup.Keys))
                {
                    this.RemoveEnemy(key);
                }

                return;
            }

            foreach (Enemy curEnemy in new List<Enemy>(this.spawnedEnemiesLookup.Keys))
            {
                if (!this.CurrentCampaignContext.CurrentCombatContext.Enemies.Contains(curEnemy))
                {
                    this.RemoveEnemy(curEnemy);
                }
            }
        }

        public void StatusEffectClicked(AppliedStatusEffect representingEffect)
        {
            if (representingEffect.BasedOnStatusEffect.WindowResponders.ContainsKey(KnownReactionWindows.Activated))
            {
                ReactionWindowContext activatedContext = new ReactionWindowContext(
                    this.CentralGameStateControllerInstance.CurrentCampaignContext,
                    KnownReactionWindows.Activated,
                    this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer, 
                    playedFromZone: "potion");

                if (representingEffect.TryGetReactionEvents(this.CentralGameStateControllerInstance.CurrentCampaignContext, activatedContext, out List<WindowResponse> responses))
                {
                    foreach (WindowResponse response in responses)
                    {
                        this.CentralGameStateControllerInstance.CurrentCampaignContext.IngestStatusEffectHappening(activatedContext, response);
                    }
                }
            }
        }

        public void OpenAllCardsBrowser()
        {
            if (this.CardBrowserUXInstance.gameObject.activeInHierarchy)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"The card browser is already open. Perhaps you need to respond to an event.", GlobalUpdateUX.LogType.UserError);
                return;
            }

            this.CardBrowserUXInstance.gameObject.SetActive(true);
            this.CardBrowserUXInstance.SetLabelText("Now Viewing: All Cards");
            this.CardBrowserUXInstance.SetFromCards(CardDatabase.GetOneOfEachCard());
            GlobalUpdateUX.UpdateUXEvent.Invoke();
        }

        public void OpenDiscardCardsBrowser()
        {
            if (this.CardBrowserUXInstance.gameObject.activeInHierarchy)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"The card browser is already open. Perhaps you need to respond to an event.", GlobalUpdateUX.LogType.UserError);
                return;
            }

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                return;
            }

            this.CardBrowserUXInstance.gameObject.SetActive(true);
            this.CardBrowserUXInstance.SetLabelText("Now Viewing: Cards in Discard");
            this.CardBrowserUXInstance.SetFromCards(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDiscard);
        }

        public void OpenDeckCardsBrowser()
        {
            if (this.CardBrowserUXInstance.gameObject.activeInHierarchy)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"The card browser is already open. Perhaps you need to respond to an event.", GlobalUpdateUX.LogType.UserError);
                return;
            }

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext == null)
            {
                return;
            }

            List<Card> cardsInDeck = new List<Card>(this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck.AllCardsInDeck);

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext != null)
            {
                cardsInDeck = new List<Card>(this.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDeck);
            }

            cardsInDeck.Sort((Card a, Card b) => a.Name.CompareTo(b.Name));

            this.CardBrowserUXInstance.gameObject.SetActive(true);
            this.CardBrowserUXInstance.SetLabelText("Now Viewing: Cards in Deck");
            this.CardBrowserUXInstance.SetFromCards(cardsInDeck);
        }

        public void OpenExileCardsBrowser()
        {
            if (this.CardBrowserUXInstance.gameObject.activeInHierarchy)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"The card browser is already open. Perhaps you need to respond to an event.", GlobalUpdateUX.LogType.UserError);
                return;
            }

            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                return;
            }

            List<Card> cardsInExile = new List<Card>(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInExile);
            cardsInExile.Sort((Card a, Card b) => a.Name.CompareTo(b.Name));

            this.CardBrowserUXInstance.gameObject.SetActive(true);
            this.CardBrowserUXInstance.SetLabelText("Now Viewing: Cards in Exile");
            this.CardBrowserUXInstance.SetFromCards(cardsInExile);
        }
    }
}