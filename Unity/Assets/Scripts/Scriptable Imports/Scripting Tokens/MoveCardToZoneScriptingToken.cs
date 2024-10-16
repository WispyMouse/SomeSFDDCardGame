namespace SFDDCards.ScriptingTokens
{
    using SFDDCards.Evaluation.Actual;
    using SFDDCards.Evaluation.Conceptual;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;

    public class MoveCardToZoneScriptingToken : BaseScriptingToken, IRealizedOperationScriptingToken
    {
        public string Zone { get; private set; }

        public override string ScriptingTokenIdentifier { get; } = "MOVECARDTOZONE";

        public override void ApplyToken(ConceptualTokenEvaluatorBuilder tokenBuilder)
        {
            if (tokenBuilder.RelevantCards == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke("Attempted to apply an action that requires relevant cards selected without any.", GlobalUpdateUX.LogType.RuntimeError);
            }

            tokenBuilder.RealizedOperationScriptingToken = this;

            ConceptualTokenEvaluatorBuilder previous = tokenBuilder.PreviousBuilder;
            do
            {
                if (previous != null && previous.RelevantCards != null && previous.RelevantCards.Equals(tokenBuilder.RelevantCards))
                {
                    if (previous.RealizedOperationScriptingToken is ILaterZoneListenerScriptingToken laterListener)
                    {
                        if (!string.IsNullOrEmpty(laterListener.LaterRealizedDestinationZone))
                        {
                            break;
                        }

                        laterListener.LaterRealizedDestinationZone = this.Zone;

                        if (laterListener.ShouldSilenceSpeaker)
                        { 

                            this.SkipDescribingMe = true;
                        }
                    }
                }
                else
                {
                    break;
                }

                previous = previous.PreviousBuilder;
            } while (previous != null);
        }

        protected override bool TryGetTokenWithArguments(List<string> arguments, out IScriptingToken scriptingToken)
        {
            scriptingToken = null;

            if (arguments.Count != 1)
            {
                return false;
            }

            scriptingToken = new MoveCardToZoneScriptingToken()
            {
                Zone = arguments[0].ToLower()
            };

            return true;
        }

        public string DescribeOperationAsEffect(ConceptualDeltaEntry delta, string reactionWindowId)
        {
            if (Zone == CardsEvaluatableValue.DiscardZoneId)
            {
                return $"Discard {delta.MadeFromBuilder.RelevantCards.DescribeEvaluation()}";
            }
            else if (Zone == CardsEvaluatableValue.ExileZoneId)
            {
                return $"Exile {delta.MadeFromBuilder.RelevantCards.DescribeEvaluation()}";
            }
            else if (Zone == CardsEvaluatableValue.DeckZoneId)
            {
                return $"Put {delta.MadeFromBuilder.RelevantCards.DescribeEvaluation()} into the deck and shuffle";
            }
            else if (Zone == CardsEvaluatableValue.HandZoneId)
            {
                if (delta.MadeFromBuilder.PlayedFromZone == "hand" && delta.MadeFromBuilder.RelevantCards is SelfCardEvaluatableValue)
                {
                    return $"Return this card to hand";
                }

                return $"Put {delta.MadeFromBuilder.RelevantCards.DescribeEvaluation()} in hand";
            }

            return $"Move {delta.MadeFromBuilder.RelevantCards.DescribeEvaluation()} to {Zone}";
        }

        public string DescribeOperationAsEffect(TokenEvaluatorBuilder builder)
        {
            if (Zone == CardsEvaluatableValue.DiscardZoneId)
            {
                return $"Discard {builder.RelevantCardsEvaluatable.DescribeEvaluation()}";
            }
            else if (Zone == CardsEvaluatableValue.ExileZoneId)
            {
                return $"Exile {builder.RelevantCardsEvaluatable.DescribeEvaluation()}";
            }
            else if (Zone == CardsEvaluatableValue.DeckZoneId)
            {
                return $"Put {builder.RelevantCardsEvaluatable.DescribeEvaluation()} into the deck and shuffle";
            }
            else if (Zone == CardsEvaluatableValue.HandZoneId)
            {
                if (builder.BasedOnConcept.PlayedFromZone == "hand" && builder.RelevantCardsEvaluatable is SelfCardEvaluatableValue)
                {
                    return $"Return this card to hand";
                }

                return $"Put {builder.RelevantCardsEvaluatable.DescribeEvaluation()} in hand";
            }

            return $"Move {builder.RelevantCardsEvaluatable.DescribeEvaluation()} to {Zone}";
        }

        public void ApplyToDelta(DeltaEntry applyingDuringEntry, ReactionWindowContext? context, out List<DeltaEntry> stackedDeltas)
        {
            stackedDeltas = null;

            if (applyingDuringEntry.RelevantCards == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Relevant cards list is null. Cannot move cards.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            if (!applyingDuringEntry.RelevantCards.TryEvaluateValue(applyingDuringEntry.FromCampaign, applyingDuringEntry.MadeFromBuilder, out List<Card> cards))
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Failed to parse relevant cards.", GlobalUpdateUX.LogType.RuntimeError);
                return;
            }

            List<Card> targetZone = null;

            if (this.Zone == CardsEvaluatableValue.DiscardZoneId)
            {
                targetZone = applyingDuringEntry.FromCampaign.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDiscard;
            }
            else if (this.Zone == CardsEvaluatableValue.HandZoneId)
            {
                targetZone = applyingDuringEntry.FromCampaign.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInHand;
            }
            else if (this.Zone == CardsEvaluatableValue.ExileZoneId)
            {
                targetZone = applyingDuringEntry.FromCampaign.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInExile;
            }
            else if (this.Zone == CardsEvaluatableValue.DeckZoneId)
            {
                targetZone = applyingDuringEntry.FromCampaign.CurrentCombatContext.PlayerCombatDeck.CardsCurrentlyInDeck;
            }

            if (targetZone == null)
            {
                GlobalUpdateUX.LogTextEvent.Invoke($"Could not determine how to move cards to zone '{this.Zone}'.", GlobalUpdateUX.LogType.RuntimeError);
            }

            foreach (Card curCard in new List<Card>(cards))
            {
                applyingDuringEntry.FromCampaign.CurrentCombatContext.PlayerCombatDeck.MoveCardToZone(curCard, targetZone);
            }
        }
    }
}