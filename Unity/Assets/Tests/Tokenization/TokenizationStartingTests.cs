namespace SpaceDeck.Tests.EditMode.Tokenization
{
    using NUnit.Framework;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using UnityEngine;
    using UnityEngine.Events;
    using SpaceDeck.Tokenization.Minimum;
    using SpaceDeck.Tokenization.Processing;

    /// <summary>
    /// This class holds tests that were made as part of a
    /// test-driven development process of the Tokenization library.
    /// 
    /// Its function serves both as a test, and a working diary
    /// of the features being considered and implemented.
    /// </summary>
    public class TokenizationStartingTests
    {
        private class HelloWorldScriptingCommand : ScriptingCommand
        {
            public static readonly LowercaseString IdentifierString = new LowercaseString("HELLOWORLD");
            public override LowercaseString Identifier => IdentifierString;
        }

        private class TwoArgumentScriptingCommand : ScriptingCommand
        {
            public static readonly LowercaseString IdentifierString = new LowercaseString("TWOARGUMENTS");
            public override LowercaseString Identifier => IdentifierString;
        }

        [TearDown]
        public void TearDown()
        {
            // Clear all Scripting Tokens
            ScriptingCommandReference.Clear();
        }

        /// <summary>
        /// This test creates and uploads a Scripting Command for Hello World.
        /// It should then be able to be fetched by its identifier.
        /// </summary>
        [Test]
        public void ScriptingCommand_Uploads()
        {
            ScriptingCommandReference.RegisterScriptingCommand(new HelloWorldScriptingCommand());
            Assert.True(ScriptingCommandReference.TryGetScriptingCommandByIdentifier(HelloWorldScriptingCommand.IdentifierString, out ScriptingCommand foundCommand), "Should be able to find scripting token by identifier.");
            Assert.IsTrue(foundCommand is HelloWorldScriptingCommand, "Found command should be of expected type.");
        }

        /// <summary>
        /// This test adds a Scripting Command for Hello World.
        /// The Token Text String provided should result in a
        /// Parsed Token holding Hello World.
        /// </summary>
        [Test]
        public void ScriptingCommand_HelloWorld_ParsedSetAsExpected()
        {
            ScriptingCommandReference.RegisterScriptingCommand(new HelloWorldScriptingCommand());

            string helloWorldTokenTextString = $"[{HelloWorldScriptingCommand.IdentifierString}]";
            Assert.True(TokenTextMaker.TryGetTokenTextFromString(helloWorldTokenTextString, out TokenText helloWorldTokenText), "Should be able to parse Token Text String into Token Text.");
            Assert.True(ParsedTokenMaker.TryGetParsedTokensFromTokenText(helloWorldTokenText, out ParsedTokenSet parsedSet), "Should be able to parse tokens from token text.");
            Assert.AreEqual(1, parsedSet.Tokens.Count, "There should be one token in the parse results, because only one token is supplied.");
            Assert.True(parsedSet.Tokens[0].CommandToExecute is HelloWorldScriptingCommand, "The token that was created should be the testing Hello World token.");
        }

        /// <summary>
        /// This test adds a Scripting Command with two arguments.
        /// Then it is tokenized and parsed.
        /// </summary>
        [Test]
        public void ScriptingCommand_TwoArgument_ParsedSetAsExpected()
        {
            ScriptingCommandReference.RegisterScriptingCommand(new TwoArgumentScriptingCommand());

            string twoArgumentTokenTextString = $"[{TwoArgumentScriptingCommand.IdentifierString}:FOO 123]";
            Assert.True(TokenTextMaker.TryGetTokenTextFromString(twoArgumentTokenTextString, out TokenText twoArgumentTokenText), "Should be able to parse Token Text String into Token Text.");
            Assert.True(ParsedTokenMaker.TryGetParsedTokensFromTokenText(twoArgumentTokenText, out ParsedTokenSet parsedSet), "Should be able to parse tokens from token text.");
            Assert.AreEqual(1, parsedSet.Tokens.Count, "There should be one token in the parse results, because only one token is supplied.");
            Assert.True(parsedSet.Tokens[0].CommandToExecute is TwoArgumentScriptingCommand, "The token that was created should be the testing Two Argument token.");
            Assert.True(parsedSet.Tokens[0].Arguments != null, "Should have arguments array, as two were supplied.");
            Assert.True(parsedSet.Tokens[0].Arguments.Count == 2, "Should have two arguments.");
            Assert.AreEqual("FOO", parsedSet.Tokens[0].Arguments[0], "Argument should be as expected in the correct order.");
            Assert.AreEqual("123", parsedSet.Tokens[0].Arguments[1], "Argument should be as expected in the correct order.");
        }
    }
}
