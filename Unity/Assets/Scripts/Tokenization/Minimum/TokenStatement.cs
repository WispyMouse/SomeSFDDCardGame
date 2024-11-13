namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Describes a sanitized string representing a Token Statement.
    /// 
    /// Through the combination of <see cref="ScriptingCommandIdentifier"/>
    /// and optionally any <see cref="Arguments"/>, this TokenStatement can be
    /// used to create one or more ParsedTokens.
    /// </summary>
    public class TokenStatement
    {
        public readonly LowercaseString ScriptingCommandIdentifier;
        public readonly List<LowercaseString> Arguments;

        public TokenTextScope ParentScope;
        public TokenStatement NextStatement;

        public TokenStatement(LowercaseString scriptingCommandIdentifier, List<LowercaseString> arguments = null)
        {
            this.ScriptingCommandIdentifier = scriptingCommandIdentifier;

            // If an argument list of zero is provided, then use null to clearly indicate the lack of arguments
            if (arguments != null && arguments.Count > 0)
            {
                this.Arguments = arguments;
            }
            else
            {
                this.Arguments = null;
            }
        }
    }
}
