namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a sanitized string representing Token Text.
    /// 
    /// Token Text is the designer's input to the Tokenization and Execution system.
    /// It contains a set of <see cref="TokenTextScope"/>s in order, with each of those
    /// containing <see cref="TokenText"/>s that have <see cref="TokenStatement"/>s.
    /// 
    /// Each TokenStatement can have its <see cref="TokenStatement.ScriptingCommandIdentifier"/>
    /// matched to a <see cref="ScriptingCommand"/>, and then <see cref="TokenStatement.Arguments"/>
    /// can be provided to form a <see cref="ParsedToken"/>. An entire TokenText parsed
    /// becomes a <see cref="ParsedTokenSet"/>.
    /// </summary>
    public struct TokenText
    {
        public readonly List<TokenTextScope> Scopes;

        public TokenText(List<TokenTextScope> scopes)
        {
            this.Scopes = scopes;
        }
    }
}
