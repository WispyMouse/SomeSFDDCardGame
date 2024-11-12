namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a sanitized string representing Token Text.
    /// 
    /// Token Text is the designer's input to the Tokenization and Execution system.
    /// It contains a set of <see cref="TokenStatement"/>s. Through the combination of a set of
    /// Statements, and matching to <see cref="ScriptingCommand"/>, <see cref="ParsedToken"/>s can
    /// be made.
    /// </summary>
    public struct TokenText
    {
        public List<TokenStatement> Statements;
    }
}
