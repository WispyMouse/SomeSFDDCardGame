namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using UnityEngine;


    public class CurrencyDatabase
    {
        public static Dictionary<string, CurrencyImport> CurrencyData { get; private set; } = new Dictionary<string, CurrencyImport>();

        public static void AddCurrencyToDatabase(CurrencyImport toImport)
        {
            CurrencyData.Add(toImport.Id.ToLower(), toImport);
        }

        public static CurrencyImport GetModel(string id)
        {
            return CurrencyData[id.ToLower()];
        }

        public static bool TryGetModel(string id, out CurrencyImport foundModel)
        {
            return CurrencyData.TryGetValue(id.ToLower(), out foundModel);
        }

        public static void ClearDatabase()
        {
            CurrencyData.Clear();
        }
    }
}