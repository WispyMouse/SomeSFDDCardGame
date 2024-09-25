namespace SFDDCards
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class EffectDescription : IEquatable<EffectDescription>
    {
        public string DescribingLabel { get; set; } = string.Empty;
        public List<string> DescriptionText { get; set; } = new List<string>();
        public HashSet<StatusEffect> MentionedStatusEffects { get; set; } = new HashSet<StatusEffect>();

        public string BreakDescriptionsIntoString()
        {
            StringBuilder descriptionBuilder = new StringBuilder();

            string leadingNewline = "";
            List<string> descriptionTexts = this.DescriptionText;
            for (int ii = 0; ii < descriptionTexts.Count; ii++)
            {
                descriptionBuilder.AppendLine($"{leadingNewline}{descriptionTexts[ii].Trim()}");
                leadingNewline = "\n";
            }

            return descriptionBuilder.ToString().Trim();
        }

        public bool Equals(EffectDescription other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.DescriptionText.Count != this.DescriptionText.Count)
            {
                return false;
            }

            for (int ii = 0; ii < other.DescriptionText.Count; ii++)
            {
                if (this.DescriptionText[ii] != other.DescriptionText[ii])
                {
                    return false;
                }
            }

            return true;
        }

        public List<EffectDescription> GetInnerDescriptions()
        {
            List<EffectDescription> innerDescriptions = new List<EffectDescription>();
            HashSet<StatusEffect> mentionedEffects = new HashSet<StatusEffect>();
            Queue<StatusEffect> statusesToMention = new Queue<StatusEffect>(this.MentionedStatusEffects);

            while (statusesToMention.Count > 0)
            {
                StatusEffect nextStatus = statusesToMention.Dequeue();

                if (mentionedEffects.Contains(nextStatus))
                {
                    continue;
                }

                EffectDescription nextStatusDescription = nextStatus.DescribeStatusEffect();
                innerDescriptions.Add(nextStatusDescription);

                foreach (StatusEffect mentionedStatusEffect in nextStatusDescription.MentionedStatusEffects)
                {
                    if (!mentionedEffects.Contains(mentionedStatusEffect))
                    {
                        mentionedEffects.Add(mentionedStatusEffect);
                        statusesToMention.Enqueue(mentionedStatusEffect);
                    }
                }
            }

            return innerDescriptions;
        }
    }
}