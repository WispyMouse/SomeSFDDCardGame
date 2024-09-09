namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    [Serializable]
    public class RunConfiguration
    {
        public List<string> StartingDeck = new List<string>();
        public int CardsToAwardOnVictory = 3;
        public int StartingMaximumHealth = 50;
        public int CardsInShop = 12;
    }
}