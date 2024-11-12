namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a stack of LinkedTokens.
    /// </summary>
    public struct LinkedTokenSet
    {
        public List<LinkedToken> Tokens;

        public LinkedTokenSet(List<LinkedToken> tokens)
        {
            this.Tokens = tokens;
        }
    }
}