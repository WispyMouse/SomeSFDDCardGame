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

        private void Awake()
        {
            this.Annihilate();
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
            this.PlayerUXInstance.SetFromPlayer(CentralGameStateControllerInstance.CurrentPlayer);

            this.LifeValue.text = $"{this.CentralGameStateControllerInstance.CurrentPlayer.CurrentHealth} / {this.CentralGameStateControllerInstance.CurrentPlayer.MaxHealth}";
        }

        public void GameCampaignNavigationStateChanged(CentralGameStateController.GameplayCampaignState newState, CentralGameStateController.TurnStatus turnStatus, CentralGameStateController.NonCombatEncounterStatus noncombatStatus)
        {
            if (newState == CentralGameStateController.GameplayCampaignState.ClearedRoom
                || (newState == CentralGameStateController.GameplayCampaignState.NonCombatEncounter && noncombatStatus == CentralGameStateController.NonCombatEncounterStatus.AllowedToLeave))
            {
                this.GoNextRoomButton.SetActive(true);
            }
            else
            {
                this.RewardsPanelUXInstance.gameObject.SetActive(false);
                this.ShopPanelUXInstance.gameObject.SetActive(false);
                this.GoNextRoomButton.SetActive(false);
            }
            
            if (newState == CentralGameStateController.GameplayCampaignState.InCombat)
            {
                this.RewardsPanelUXInstance.gameObject.SetActive(false);
                this.ShopPanelUXInstance.gameObject.SetActive(false);

                if (turnStatus == CentralGameStateController.TurnStatus.PlayerTurn)
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

            this.UpdateUX();
        }

        public void UpdateUX()
        {
            this.SetElementValueLabel();
            this.UpdateEnemyUX();
            this.UpdatePlayerLabelValues();
            this.RepresentPlayerHand();
            this.RepresentTargetables();
        }

        public void SelectTarget(ICombatantTarget toSelect)
        {
            if (this.CurrentSelectedCard == null)
            {
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentTurnStatus != CentralGameStateController.TurnStatus.PlayerTurn)
            {
                return;
            }

            this.CentralGameStateControllerInstance.PlayCard(this.CurrentSelectedCard.RepresentedCard, toSelect);
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
            EnemyUX representation = this.spawnedEnemiesLookup[toRemove];
            Destroy(representation.gameObject);
            this.spawnedEnemiesLookup.Remove(toRemove);
        }

        public void SelectCurrentCard(DisplayedCardUX toSelect)
        {
            if (this.CentralGameStateControllerInstance.CurrentTurnStatus != CentralGameStateController.TurnStatus.PlayerTurn)
            {
                return;
            }

            if (this.PlayerIsCurrentlyAnimating)
            {
                this.AddToLog($"Player is currently animating, please wait until finished. (Being able to play faster will be fixed soon!)");
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
                if (builder.MeetsElementRequirements(this.CentralGameStateControllerInstance))
                {
                    anyPassingRequirements = true;
                    break;
                }
            }

            if (!anyPassingRequirements)
            {
                this.AddToLog($"Unable to play card {toSelect.RepresentedCard.Name}. No requirements for any of the card's effects have been met.");
                return;
            }

            this.CurrentSelectedCard = toSelect;
            this.CurrentSelectedCard.EnableSelectionGlow();
            this.AppointTargetableIndicatorsToValidTargets(toSelect.RepresentedCard);
            AddToLog($"Selected card {toSelect.RepresentedCard.Name}");
        }

        public void AddToLog(string text)
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
        }

        public void ShowShopPanel(params Card[] cardsInShop)
        {
            this.ShopPanelUXInstance.gameObject.SetActive(true);
            this.ShopPanelUXInstance.SetRewardCards(cardsInShop);
        }

        private IEnumerator AnimateEnemyTurnsInternal(Action continuationAction)
        {
            foreach (Enemy curEnemy in this.CentralGameStateControllerInstance.CurrentRoom.Enemies)
            {
                yield return AnimateAction(this.spawnedEnemiesLookup[curEnemy], curEnemy.Intent, curEnemy.Intent.PrecalculatedTarget, () => { this.CentralGameStateControllerInstance.EnemyActsOnIntent(curEnemy); });
            }

            continuationAction.Invoke();
        }

        public void AnimateCardPlay(Card toPlay, ICombatantTarget target, Action executionAction, Action continuationAction)
        {
            this.StartCoroutine(AnimateCardPlayInternal(toPlay, target, executionAction, continuationAction));
        }

        private IEnumerator AnimateCardPlayInternal(Card toPlay, ICombatantTarget target, Action executionAction, Action continuationAction)
        {
            PlayerIsCurrentlyAnimating = true;

            yield return AnimateAction(this.PlayerUXInstance, toPlay, target, executionAction);

            PlayerIsCurrentlyAnimating = false;
            continuationAction.Invoke();
        }

        private IEnumerator AnimateAction(IAnimationPuppet puppet, IAttackTokenHolder attack, ICombatantTarget target, Action executionAction)
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
                    puppet,
                    executionAction
                );
            }
            else
            {
                yield return this.AnimationRunnerController.AnimateShoveAttack(
                    puppet,
                    targetPuppet,
                    executionAction
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

            if (this.CentralGameStateControllerInstance.CurrentGameplayCampaignState != CentralGameStateController.GameplayCampaignState.InCombat)
            {
                if (this.CentralGameStateControllerInstance.CurrentDeck == null)
                {
                    this.CardsInDeckValue.text = "0";
                }
                else
                {
                    this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentDeck.AllCardsInDeck.Count.ToString();
                }
                this.CardsInDiscardValue.text = "0";
                ClearAllTargetableIndicators();
                return;
            }

            float leftStartingPoint = -CardFanDistance * (this.CentralGameStateControllerInstance.CurrentDeck.CardsCurrentlyInHand.Count - 1) / 2f;

            for (int ii = 0; ii < this.CentralGameStateControllerInstance.CurrentDeck.CardsCurrentlyInHand.Count; ii++)
            {
                Vector3 objectOffset = new Vector3(leftStartingPoint, 0, 0) + new Vector3(CardFanDistance, 0, 0) * ii;
                CombatCardUX newCard = Instantiate(this.CardRepresentationPF, this.PlayerHandTransform);
                newCard.transform.localPosition = objectOffset;
                newCard.SetFromCard(this.CentralGameStateControllerInstance.CurrentDeck.CardsCurrentlyInHand[ii], SelectCurrentCard);
            }

            if (this.CentralGameStateControllerInstance.CurrentGameplayCampaignState == CentralGameStateController.GameplayCampaignState.InCombat)
            {
                this.CardsInDeckValue.text = this.CentralGameStateControllerInstance.CurrentDeck.CardsCurrentlyInDeck.Count.ToString();
                this.CardsInDiscardValue.text = this.CentralGameStateControllerInstance.CurrentDeck.CardsCurrentlyInDiscard.Count.ToString();
            }
        }

        void UpdatePlayerLabelValues()
        {
            if (this.CentralGameStateControllerInstance.CurrentPlayer == null)
            {
                return;
            }

            this.LifeValue.text = this.CentralGameStateControllerInstance.CurrentPlayer.CurrentHealth.ToString();
        }

        private void SetElementValueLabel()
        {
            if (this.CentralGameStateControllerInstance.ElementResourceCounts == null || this.CentralGameStateControllerInstance.ElementResourceCounts.Count == 0)
            {
                this.ElementsValue.text = "None";
                return;
            }

            string startingSeparator = "";
            StringBuilder compositeElements = new StringBuilder();
            foreach (string element in this.CentralGameStateControllerInstance.ElementResourceCounts.Keys)
            {
                compositeElements.Append($"{startingSeparator}{this.CentralGameStateControllerInstance.ElementResourceCounts[element]}\u00A0{element}");
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
            possibleTargets.Add(this.CentralGameStateControllerInstance.CurrentPlayer);

            foreach (Enemy curEnemy in this.spawnedEnemiesLookup.Keys)
            {
                possibleTargets.Add(curEnemy);
            }

            List<ICombatantTarget> remainingTargets = ScriptTokenEvaluator.GetTargetsThatCanBeTargeted(this.CentralGameStateControllerInstance.CurrentPlayer, toTarget, possibleTargets);

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
            if (this.CentralGameStateControllerInstance.CurrentRoom == null || this.CentralGameStateControllerInstance.CurrentRoom.Enemies.Count == 0)
            {
                return;
            }

            foreach (Enemy curEnemy in this.CentralGameStateControllerInstance.CurrentRoom.Enemies)
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
            this.UpdateUX();
        }

        void RepresentTargetables()
        {
            if (this.CentralGameStateControllerInstance.CurrentGameplayCampaignState != CentralGameStateController.GameplayCampaignState.InCombat)
            {
                ClearAllTargetableIndicators();
                return;
            }

            if (this.CentralGameStateControllerInstance.CurrentTurnStatus != CentralGameStateController.TurnStatus.PlayerTurn)
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
    }
}