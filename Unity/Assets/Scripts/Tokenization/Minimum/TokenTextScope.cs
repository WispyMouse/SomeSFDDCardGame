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
    public class TokenTextScope
    {
        /// <summary>
        /// An ordered list of statements within this scope.
        /// </summary>
        public readonly List<TokenStatement> Statements;

        public TokenStatement NextStatementAfterScope;

        public TokenTextScope(List<TokenStatement> statements)
        {
            this.Statements = statements;
        }
    }
}
