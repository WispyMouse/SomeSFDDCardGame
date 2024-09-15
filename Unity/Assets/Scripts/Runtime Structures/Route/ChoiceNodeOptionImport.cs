namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class ChoiceNodeOptionImport
    {
        public string ChoiceNodeKind { get; set; }
        public List<string> ChoiceNodeArguments { get; set; } = new List<string>();
    }
}