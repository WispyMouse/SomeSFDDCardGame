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
        public const string Activated = "activated";
        public const string ConsideringPlayingFromHand = "consideringplayingfromhand";
        public const string LookingNotPlaying = "lookingnotplaying";

        public const string IncomingIntensity = "incomingintensity";
        public const string DamageIncoming = "incomingdamage";
        public const string DamageDealt = "damagedealt";

        public static ReactionWindowSubscription ParseWindow(string window, AppliedStatusEffect appliedEffect)
        {
            switch (window.ToLower())
            {
                case OwnerStartsTurn:
                    return new OwnerStartsTurnReactionWindowSubscription(appliedEffect);
                case OwnerEndsTurn:
                    return new OwnerEndsTurnReactionWindowSubscription(appliedEffect);
                case DamageIncoming:
                    return new IncomingDamageReactionWindowSubscription(appliedEffect);
                case DamageDealt:
                    return new OwnerDealsDamageReactionWindowSubscription(appliedEffect);
                case Activated:
                    return null;
            }

            Debug.LogError($"{nameof(KnownReactionWindows)} ({nameof(ParseWindow)}): Failed to parse timing window '{window}'.");
            return null;
        }

        public static string GetWindowDescriptor(string window)
        {
            switch (window.ToLower())
            {
                case OwnerStartsTurn:
                    return "Start of turn";
                case OwnerEndsTurn:
                    return "End of turn";
                case Activated:
                    return "Activated";
                case DamageIncoming:
                    return "Incoming Damage";
                case DamageDealt:
                    return "Owner Deals Damage";
                case "testwindow":
                    return "";
            }

            Debug.LogError($"{nameof(KnownReactionWindows)} ({nameof(ParseWindow)}): Failed to describe timing window '{window}'.");
            return null;
        }
    }
}