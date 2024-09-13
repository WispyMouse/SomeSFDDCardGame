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

        public CampaignContext FromCampaign;
        public readonly Encounter BasedOnEncounter;
        public readonly List<Enemy> Enemies = new List<Enemy>();

        private readonly GameplayUXController UXController = null;

        public CombatContext(CampaignContext fromCampaign, Encounter basedOnEncounter, GameplayUXController uxController)
        {
            this.UXController = uxController;

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

            UpdateUXGlobalEvent.UpdateUXEvent.Invoke();
        }

        public void EndCurrentTurnAndChangeTurn(TurnStatus toTurn)
        {
            this.CurrentTurnStatus = toTurn;

            // If an enemy doesn't have an intent, set it regardless of the phase we're moving towards.
            this.AssignEnemyIntents();

            if (toTurn == TurnStatus.PlayerTurn)
            {
                const int playerhandsize = 5;
                CombatTurnController.PushSequenceToTop(new GameplaySequenceEvent(
                        () => this.PlayerCombatDeck.DealCards(playerhandsize),
                        null
                ));
            }
            else if (toTurn == TurnStatus.EnemyTurn)
            {
                foreach (Enemy curEnemy in new List<Enemy>(this.Enemies))
                {
                    CombatTurnController.PushSequencesToTop(
                        new GameplaySequenceEvent(() =>
                        {
                            this.EnemyAction(curEnemy, curEnemy?.Intent?.PrecalculatedTarget);
                        }),
                        new GameplaySequenceEvent(() =>
                        {
                            this.EndCurrentTurnAndChangeTurn(TurnStatus.PlayerTurn);
                        },
                        null));
                }
            }

            UpdateUXGlobalEvent.UpdateUXEvent.Invoke();
            return;
        }

        /// <summary>
        /// Plays a specified card on the specified target.
        /// </summary>
        public void PlayCard(Card toPlay, ICombatantTarget toPlayOn)
        {
            if (this.CurrentTurnStatus != TurnStatus.PlayerTurn)
            {
                UXController.AddToLog($"Card {toPlay.Name} can't be played, it isn't the player's turn.");
                return;
            }

            if (!this.PlayerCombatDeck.CardsCurrentlyInHand.Contains(toPlay))
            {
                UXController.AddToLog($"Card {toPlay.Name} appears to not be in hand. Has it already been played?");
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
                UXController.AddToLog($"Unable to play card {toPlay.Name}. No requirements for any of the card's effects have been met.");
                UXController.CancelAllSelections();
                return;
            }

            UXController.AddToLog($"Playing card {toPlay.Name} on {toPlayOn.Name}");
            UXController.CancelAllSelections();
            this.PlayerCombatDeck.CardsCurrentlyInHand.Remove(toPlay);

            CombatTurnController.PushSequenceToTop(new GameplaySequenceEvent(
            () =>
            {
                GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this.FromCampaign, this.FromCampaign.CampaignPlayer, toPlay, toPlayOn);
                UXController.AddToLog(delta.DescribeDelta());
                delta.ApplyDelta(this.FromCampaign, UXController.AddToLog);
                this.CheckAllStateEffectsAndKnockouts();
            },
            () => UXController.AnimateCardPlay(
                toPlay,
                toPlayOn)
            ));
        }

        public void EnemyAction(Enemy toAct, ICombatantTarget target)
        {
            if (toAct?.Intent == null)
            {
                UXController.AddToLog($"Enemy {toAct.Name} has no Intent, cannot take action.");
                return;
            }

            GamestateDelta delta = ScriptTokenEvaluator.CalculateDifferenceFromTokenEvaluation(this.FromCampaign, toAct, toAct.Intent, this.FromCampaign.CampaignPlayer);
            UXController.AddToLog(delta.DescribeDelta());
            delta.ApplyDelta(this.FromCampaign, UXController.AddToLog);

            this.CheckAllStateEffectsAndKnockouts();
        }

        private void InitializeStartingEnemies()
        {
            foreach (string curEnemyId in this.BasedOnEncounter.EnemiesInEncounterById)
            {
                EnemyModel curEnemyModel = EnemyDatabase.GetModel(curEnemyId);
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

                int randomAttackIndex = UnityEngine.Random.Range(0, curEnemy.BaseModel.Attacks.Count);
                EnemyAttack randomAttack = curEnemy.BaseModel.Attacks[randomAttackIndex];
                curEnemy.Intent = randomAttack;

                List<ICombatantTarget> consideredTargets = new List<ICombatantTarget>()
                {
                    curEnemy,
                    this.FromCampaign.CampaignPlayer
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

            UpdateUXGlobalEvent.UpdateUXEvent.Invoke();
        }

        public void CheckAllStateEffectsAndKnockouts()
        {
            if (this.FromCampaign.CampaignPlayer.CurrentHealth <= 0)
            {
                this.UXController.AddToLog($"The player has run out of health! This run is over.");
                this.FromCampaign.SetCampaignState(CampaignContext.GameplayCampaignState.Defeat);

                CombatTurnController.StopAllSequences();

                return;
            }

            List<Enemy> enemies = new List<Enemy>(this.Enemies);
            foreach (Enemy curEnemy in enemies)
            {
                if (curEnemy.ShouldBecomeDefeated && !curEnemy.DefeatHasBeenSignaled)
                {
                    curEnemy.DefeatHasBeenSignaled = true;

                    this.UXController.AddToLog($"{curEnemy.Name} has been defeated!");

                    CombatTurnController.PushSequenceToTop(new GameplaySequenceEvent(
                        () =>
                        {
                            this.UXController.RemoveEnemy(curEnemy);
                            this.Enemies.Remove(curEnemy);
                            this.CheckAllStateEffectsAndKnockouts();
                        },
                        null));
                }
            }

            if (this.FromCampaign.CurrentNonCombatEncounterStatus == CampaignContext.NonCombatEncounterStatus.NotInNonCombatEncounter && enemies.Count == 0)
            {
                this.UXController.AddToLog($"There are no more enemies!");
                this.FromCampaign.SetCampaignState(CampaignContext.GameplayCampaignState.ClearedRoom);
                // this.SetupClearedRoomAndPresentAwards();
                return;
            }

            this.UXController.UpdateUX();
        }
    }
}