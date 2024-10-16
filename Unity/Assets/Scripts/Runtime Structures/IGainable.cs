namespace SFDDCards
{
    using SFDDCards.ImportModels;
    using SFDDCards.ScriptingTokens.EvaluatableValues;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;

    /// <summary>
    /// Represents some amount of something to gain.
    /// Only one of <see cref="GainedEffect"/>, <see cref="GainedCard"/>, or <see cref="GainedCurrency"/> should return a value.
    /// The rest should be null.
    /// Callers can act as though only one has a value, in the above order.
    /// </summary>
    public interface IGainable
    {
        StatusEffect GainedEffect { get; }
        Card GainedCard { get; }
        CurrencyImport GainedCurrency { get; }

        IEvaluatableValue<int> GainedAmount { get; }
    }
}