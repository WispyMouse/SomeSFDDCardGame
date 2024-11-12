namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a stack of <see cref="ParsedToken"/>s.
    /// </summary>
    public struct ParsedTokenSet
    {
        public List<ParsedToken> Tokens;

        public ParsedTokenSet(List<ParsedToken> tokens)
        {
            this.Tokens = tokens;
        }
    }
}