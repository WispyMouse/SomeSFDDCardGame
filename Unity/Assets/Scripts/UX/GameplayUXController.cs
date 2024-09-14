namespace SFDDCards
{
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
        private RewardsPanelUX RewardsPanelUXInstance;
        [SerializeReference]
        private ShopUX ShopPanelUXInstance;

        [SerializeReference]
        private PlayerUX PlayerRepresentationPF;
        private PlayerUX PlayerUXInstance { get; set; }

        [SerializeReference]
        private EnemyUX EnemyRepresentationPF;
        [SerializeReference]
        private CombatCardUX CardRepresentationPF;
        [SerializeReference]
        private Transform PlayerHandTransform;
        [SerializeReference]
        private Transform PlayerRepresentationTransform;
        [SerializeReference]
        private Transform EnemyRepresntationTransform;

        [SerializeReference]
        private GameObject GoNextRoomButton;
        [SerializeReference]
        private GameObject ResetGameButton;
        [SerializeReference]
        private GameObject EndTurnButton;

        [SerializeReference]
        private GameObject DeckStatPanel;
        [SerializeReference]
        private TMPro.TMP_Text CardsInDeckValue;
        [SerializeReference]
        private TMPro.TMP_Text CardsInDiscardValue;
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
        private TMPro.TMP_Text Log;

        private Dictionary<Enemy, EnemyUX> spawnedEnemiesLookup { get; set; } = new Dictionary<Enemy, EnemyUX>();

        public DisplayedCardUX CurrentSelectedCard { get; private set; } = null;
        public bool PlayerIsCurrentlyAnimating { get; private set; } = false;

        private CampaignContext.GameplayCampaignState previousCampaignState { get; set; } = CampaignContext.GameplayCampaignState.NotStarted;
        private CombatContext.TurnStatus previousCombatTurnState { get; set; } = CombatContext.TurnStatus.NotInCombat;
        private CampaignContext.NonCombatEncounterStatus previousNonCombatEncounterState { get; set; } = CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter;

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
            this.PlayerUXInstance.SetFromPlayer(this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer);

            this.LifeValue.text = $"{this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer.CurrentHealth} / {this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignPlayer.MaxHealth}";
        }

        public void CheckAndActIfGameCampaignNavigationStateChanged()
        {
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext == null)
            {
                return;
            }

            CampaignContext.GameplayCampaignState newCampaignState = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentGameplayCampaignState;
            CampaignContext.NonCombatEncounterStatus newNonCombatState = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentNonCombatEncounterStatus;
            CombatContext.TurnStatus newTurnState = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null ? CombatContext.TurnStatus.NotInCombat : this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus;

            if (this.previousCampaignState == newCampaignState
                && this.previousNonCombatEncounterState == newNonCombatState
                && this.previousCombatTurnState == newTurnState)
            {
                return;
            }

            if (newCampaignState == CampaignContext.GameplayCampaignState.ClearedRoom
                || (newCampaignState == CampaignContext.GameplayCampaignState.NonCombatEncounter && newNonCombatState == CampaignContext.NonCombatEncounterStatus.AllowedToLeave))
            {
                this.GoNextRoomButton.SetActive(true);
            }
            else
            {
                this.RewardsPanelUXInstance.gameObject.SetActive(false);
                this.ShopPanelUXInstance.gameObject.SetActive(false);
                this.GoNextRoomButton.SetActive(false);
            }
            
            if (newCampaignState == CampaignContext.GameplayCampaignState.InCombat)
            {
                this.RewardsPanelUXInstance.gameObject.SetActive(false);
                this.ShopPanelUXInstance.gameObject.SetActive(false);

                if (newTurnState == CombatContext.TurnStatus.PlayerTurn)
                {
                    this.EndTurnButton.SetActive(true);
                }
                else
                {
                    this.EndTurnButton.SetActive(false);
                }
            }
            else
            {
                this.EndTurnButton.SetActive(false);
            }

            this.previousCampaignState = newCampaignState;
            this.previousNonCombatEncounterState = newNonCombatState;
            this.previousCombatTurnState = newTurnState;
        }

        public void UpdateUX()
        {
            this.CheckAndActIfGameCampaignNavigationStateChanged();
            this.SetElementValueLabel();
            this.UpdateEnemyUX();
            this.UpdatePlayerLabelValues();
            this.RepresentPlayerHand();
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
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null ||
                this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
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

            // Does the player meet the requirements of at least one of the effects?
            bool anyPassingRequirements = false;
            List<TokenEvaluatorBuilder> builders = ScriptTokenEvaluator.CalculateEvaluatorBuildersFromTokenEvaluation(toSelect.RepresentedCard);
            foreach (TokenEvaluatorBuilder builder in builders)
            {
                if (builder.MeetsElementRequirements(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext))
                {
                    anyPassingRequirements = true;
                    break;
                }
            }

            if (!anyPassingRequirements)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Unable to play card {toSelect.RepresentedCard.Name}. No requirements for any of the card's effects have been met.", GlobalUpdateUX.LogType.GameEvent);
                return;
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

            ClearAllTargetableIndicators();
        }

        public void AnimateEnemyTurns(Action continuationAction)
        {
            this.StartCoroutine(AnimateEnemyTurnsInternal(continuationAction));
        }

        public void ShowRewardsPanel(params Card[] cardsToReward)
        {
            this.RewardsPanelUXInstance.gameObject.SetActive(true);
            this.RewardsPanelUXInstance.SetRewardCards(cardsToReward);
            this.UpdateUX();
        }

        public void ShowShopPanel(params Card[] cardsInShop)
        {
            this.ShopPanelUXInstance.gameObject.SetActive(true);
            this.ShopPanelUXInstance.SetRewardCards(cardsInShop);
        }

        private IEnumerator AnimateEnemyTurnsInternal(Action continuationAction)
        {
            foreach (Enemy curEnemy in this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.Enemies)
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

        /// <summary>
        /// Takes the current player hand, makes the appropriate cards, and sets them in the player's hand.
        /// </summary>
        void RepresentPlayerHand()
        {
            const float CardFanDistance = 1.5f;

            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext == null)
            {
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext == null)
            {
                this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck.AllCardsInDeck.Count.ToString();
                this.CardsInDiscardValue.text = "0";
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentGameplayCampaignState != CampaignContext.GameplayCampaignState.InCombat)
            {
                if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck == null)
                {
                    this.CardsInDeckValue.text = "0";
                }
                else
                {
                    this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CampaignDeck.AllCardsInDeck.Count.ToString();
                }
                this.CardsInDiscardValue.text = "0";
                ClearAllTargetableIndicators();
                return;
            }

            float leftStartingPoint = -CardFanDistance * (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand.Count - 1) / 2f;

            for (int ii = 0; ii < this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand.Count; ii++)
            {
                Vector3 objectOffset = new Vector3(leftStartingPoint, 0, 0) + new Vector3(CardFanDistance, 0, 0) * ii;
                CombatCardUX newCard = Instantiate(this.CardRepresentationPF, this.PlayerHandTransform);
                newCard.transform.localPosition = objectOffset;
                newCard.SetFromCard(this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand[ii], SelectCurrentCard);
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentGameplayCampaignState == CampaignContext.GameplayCampaignState.InCombat)
            {
                this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDeck.Count.ToString();
                this.CardsInDiscardValue.text = this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDiscard.Count.ToString();
            }
        }

        void UpdatePlayerLabelValues()
        {
            if (this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer == null)
            {
                return;
            }

            this.LifeValue.text = this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer.CurrentHealth.ToString();
            this.PlayerStatusEffectUXHolderInstance.SetStatusEffects(this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer.AppliedStatusEffects);
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
            foreach (string element in this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.ElementResourceCounts.Keys)
            {
                compositeElements.Append($"{startingSeparator}{this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.ElementResourceCounts[element]}\u00A0{element}");
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

            List<ICombatantTarget> remainingTargets = ScriptTokenEvaluator.GetTargetsThatCanBeTargeted(this.CentralGameStateControllerInstance?.CurrentCampaignContext?.CampaignPlayer, toTarget, possibleTargets);

            if (remainingTargets.Count > 0)
            {
                foreach (ICombatantTarget target in remainingTargets)
                {
                    TargetableIndicator indicator = Instantiate(this.SingleCombatantTargetableIndicatorPF, target.UXPositionalTransform);
                    indicator.SetFromTarget(target, this.SelectTarget);
                    this.ActiveIndicators.Add(indicator);
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
            for (int ii = this.PlayerHandTransform.childCount - 1; ii >= 0; ii--)
            {
                Destroy(this.PlayerHandTransform.GetChild(ii).gameObject);
            }

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
            if (this.CentralGameStateControllerInstance.CurrentCampaignContext?.CurrentCombatContext == null)
            {
                ClearAllTargetableIndicators();
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentGameplayCampaignState != CampaignContext.GameplayCampaignState.InCombat)
            {
                ClearAllTargetableIndicators();
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentCampaignContext.CurrentCombatContext.CurrentTurnStatus != CombatContext.TurnStatus.PlayerTurn)
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

        public void PresentAwards()
        {
            this.CombatTurnCounterInstance.EndHandlingCombat();
            List<Card> cardsToAward = CardDatabase.GetRandomCards(this.CentralGameStateControllerInstance.CurrentRunConfiguration.CardsToAwardOnVictory);
            this.ShowRewardsPanel(cardsToAward.ToArray());
        }
    }
}