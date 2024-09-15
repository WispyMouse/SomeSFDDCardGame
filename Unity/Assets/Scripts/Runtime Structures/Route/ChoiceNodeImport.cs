namespace SFDDCards
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class ChoiceNodeImport
    {
        public string NodeName { get; set; }
        public List<ChoiceNodeOptionImport> Options { get; set; }
    }
}