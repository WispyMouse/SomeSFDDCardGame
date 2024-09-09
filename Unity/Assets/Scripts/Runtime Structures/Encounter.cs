namespace SFDDCards
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Encounter
    {
        public string Id;
        public List<string> EnemiesInEncounterById { get; set; } = new List<string>();
        public bool IsShopEncounter;
    }
}