namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    [System.Serializable]
    public class CostEvaluationModifier
    {
        public List<string> TagMatch;
        public string Currency;
        public string EvaluationScript;
    }
}