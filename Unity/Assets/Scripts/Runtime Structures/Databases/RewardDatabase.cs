namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class RewardDatabase
    {
        public static Dictionary<string, RewardImport> RewardModels = new Dictionary<string, RewardImport>();

        public static void AddRewardToDatabase(RewardImport toAdd)
        {
            RewardModels.Add(toAdd.Id, toAdd);
        }

        public static bool TryGetReward(string rewardId, out RewardImport model)
        {
            if (RewardModels.TryGetValue(rewardId, out model))
            {
                return true;
            }

            return false;
        }

        public static Reward SaturateReward(RewardImport basedOn)
        {
            Reward saturatedRewards = new Reward(basedOn);

            RandomDecider<CardImport> cardDecider = new RandomDecider<CardImport>();
            RandomDecider<StatusEffect> statusEffectDecider = new RandomDecider<StatusEffect>();

            foreach (PickRewardImport pickRewardImport in basedOn.PickRewards)
            {
                PickSomeReward pickSomeReward = new PickSomeReward(pickRewardImport);

                foreach (RewardIdentity curIdentity in pickRewardImport.RewardIdentities)
                {
                    PickSomeRewardSlot slot = new PickSomeRewardSlot();

                    if (curIdentity.IdentityKind == RewardIdentity.RewardIdentityKind.Artifact)
                    {
                        slot.GainedEffect = StatusEffectDatabase.GetModel(curIdentity.RewardIdentifier, statusEffectDecider);
                    }
                    else if (curIdentity.IdentityKind == RewardIdentity.RewardIdentityKind.Card)
                    {
                        slot.GainedCard = CardDatabase.GetModel(curIdentity.RewardIdentifier, cardDecider);
                    }
                    else if (curIdentity.IdentityKind == RewardIdentity.RewardIdentityKind.Currency)
                    {
                        slot.GainedCurrency = CurrencyDatabase.GetModel(curIdentity.RewardIdentifier);
                    }

                    IEvaluatableValue<int> amountToAward = null;
                    if (!string.IsNullOrEmpty(curIdentity.QuantityText) && BaseScriptingToken.TryGetIntegerEvaluatableFromString(curIdentity.QuantityText, out amountToAward))
                    {
                        slot.GainedAmount = amountToAward;
                    }
                    else
                    {
                        slot.GainedAmount = new ConstantNumericEvaluatableValue(1);
                    }

                    pickSomeReward.PickRewardSlots.Add(slot);
                }

                saturatedRewards.PickRewards.Add(pickSomeReward);
            }

            return saturatedRewards;
        }
    }
}
