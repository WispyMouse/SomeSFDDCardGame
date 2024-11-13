namespace SpaceDeck.Tokenization.Processing
{
    using SpaceDeck.Tokenization.Minimum;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Static helper class for turning Token Text Strings into <see cref="TokenText"/>.
    /// </summary>
    public static class TokenTextMaker
    {
        public static bool TryGetTokenTextFromString(string tokenTextString, out TokenText createdTokenText)
        {
            createdTokenText = default(TokenText);
            return false;
        }
    }
}