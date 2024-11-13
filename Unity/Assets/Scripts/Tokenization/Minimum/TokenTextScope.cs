namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a scope of <see cref="TokenText"/>.
    /// 
    /// A scope is an associated group of <see cref="TokenStatement"/>s.
    /// A chain of scopes is <see cref="TokenText"/>.
    /// </summary>
    public struct TokenTextScope
    {
        public List<TokenStatement> Statements;
        public TokenTextScope? PreviousScope;
        public TokenTextScope? NextScope;

        public TokenTextScope(List<TokenStatement> statements, TokenTextScope? previousScope = null, TokenTextScope? nextScope = null)
        {
            this.Statements = statements;
            this.PreviousScope = previousScope;
            this.NextScope = nextScope;
        }
    }
}
