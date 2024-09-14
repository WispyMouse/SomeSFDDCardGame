namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public static class KnownReactionWindows
    {
        public const string OwnerStartsTurn = "ownerstartsturn";
        public const string OwnerEndsTurn = "ownerendsturn";

        public static ReactionWindowSubscription ParseWindow(string window, AppliedStatusEffect appliedEffect)
        {
            switch (window.ToLower())
            {
                case OwnerStartsTurn:
                    return new OwnerStartsTurnReactionWindowSubscription(appliedEffect);
                case OwnerEndsTurn:
                    return new OwnerEndsTurnReactionWindowSubscription(appliedEffect);
            }

            Debug.LogError($"{nameof(KnownReactionWindows)} ({nameof(ParseWindow)}): Failed to parse timing window '{window}'.");
            return null;
        }
    }
}