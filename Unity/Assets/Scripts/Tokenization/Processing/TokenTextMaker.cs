namespace SpaceDeck.Tokenization.Processing
{
    using SpaceDeck.Tokenization.Minimum;
    using System;
    using System.Runtime;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Static helper class for turning Token Text Strings into <see cref="TokenText"/>.
    /// </summary>
    public static class TokenTextMaker
    {
        const string TokenStatementRegex = @"^\[[^\]]*\]";

        public static bool TryGetTokenTextFromString(string tokenTextString, out TokenText createdTokenText)
        {
            if (string.IsNullOrWhiteSpace(tokenTextString))
            {
                createdTokenText = default(TokenText);
                return false;
            }

            List<TokenTextScope> scopes = new List<TokenTextScope>();

            // This stack identifies what the next scope should be after popping a scope
            Stack<TokenTextScope> scopeStack = new Stack<TokenTextScope>();

            TokenStatement previousStatement = null;
            TokenTextScope previousScopeThatHasJustEnded = null;
            TokenTextScope currentScope = new TokenTextScope(new List<TokenStatement>());
            scopes.Add(currentScope);
            scopeStack.Push(currentScope);

            // To tokenize a string, we can read the string sequentially
            // If we run into a new scope, we push a scope on the stack. These are all identified with {
            // when we run into an end of a scope, we pop a scope. These are all identified with }
            // Otherwise, everything is meant to be in the form of [SCRIPTINGCOMMANDIDENTIFIER:MAYBE ARGUMENTS]
            // Just keep reading from the string until we're done
            int tokenTextPointer = 0;
            while (tokenTextPointer < tokenTextString.Length)
            {
                // Skip whitespace
                while (tokenTextPointer < tokenTextString.Length && tokenTextString[tokenTextPointer] == ' ')
                {
                    tokenTextPointer++;
                }

                // Is this a scope identifier?
                if (tokenTextString[tokenTextPointer] == '{')
                {
                    currentScope = new TokenTextScope(new List<TokenStatement>());
                    scopes.Add(currentScope);
                    scopeStack.Push(currentScope);
                }
                // Is this a scope ender?
                else if (tokenTextString[tokenTextPointer] == '}')
                {
                    if (scopeStack.Count == 0)
                    {
                        // There's no scope on the stack, so we can't pop the current scope
                        // TODO Log why this failed
                        createdTokenText = default(TokenText);
                        return false;
                    }

                    previousScopeThatHasJustEnded = currentScope;
                    scopeStack.Pop();

                    if (scopeStack.Count == 0)
                    {
                        // There's no scope on the stack, so we can't pop the current scope
                        // TODO Log why this failed
                        createdTokenText = default(TokenText);
                        return false;
                    }

                    currentScope = scopeStack.Peek();
                }
                // No, so try to match a TokenStatement
                else
                {
                    Match statementRegexMatch = Regex.Match(tokenTextString.Substring(tokenTextPointer), TokenStatementRegex);
                    if (!statementRegexMatch.Success)
                    {
                        // We've failed to parse this.
                        // TODO: Log this out
                        createdTokenText = default(TokenText);
                        return false;
                    }

                    if (!TryGetTokenStatementFromString(statementRegexMatch.Groups[0].Value, out TokenStatement statement))
                    {
                        // We've failed to parse this.
                        // TODO: Log this out
                        createdTokenText = default(TokenText);
                        return false;
                    }

                    statement.ParentScope = currentScope;
                    currentScope.Statements.Add(statement);

                    if (previousStatement != null)
                    {
                        previousStatement.NextStatement = statement;
                    }

                    if (previousScopeThatHasJustEnded != null)
                    {
                        previousScopeThatHasJustEnded.NextStatementAfterScope = statement;
                        previousScopeThatHasJustEnded = null;
                    }

                    previousStatement = statement;

                    tokenTextPointer += statementRegexMatch.Groups[0].Value.Length - 1;
                }

                tokenTextPointer++;
            }

            createdTokenText = new TokenText(scopes);
            return true;
        }

        public static bool TryGetTokenStatementFromString(string tokenStatementString, out TokenStatement statement)
        {
            // Strip away the '[' and ']'
            string workingString = tokenStatementString.TrimStart('[').TrimEnd(']');

            // Is there any ':'? If not, then there are no arguments.
            if (!workingString.Contains(":"))
            {
                statement = new TokenStatement(workingString);
                return true;
            }

            // Split the identifier out
            string identifier = workingString.Substring(0, workingString.IndexOf(':'));
            string arguments = workingString.Substring(workingString.IndexOf(':') + 1);

            // Split the arugments by spaces
            string[] argumentArray = arguments.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            statement = new TokenStatement(identifier, LowercaseString.FromArray(argumentArray));
            return true;
        }
    }
}