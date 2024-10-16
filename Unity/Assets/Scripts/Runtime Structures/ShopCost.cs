namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class ShopCost
    {
        public IEvaluatableValue<int> Amount;

        public CurrencyImport Currency;
    }
}
