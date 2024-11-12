namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// The combination of a <see cref="ScriptingCommand"/>
    /// and arguments provided by a <see cref="TokenStatement"/>.
    /// 
    /// This provides an instruction. Unlike <see cref="ScriptingCommand"/>
    /// it has parameters, but in order to be executed it must be provided
    /// with context.
    /// 
    /// This form should be held in memory during the early stage of importing
    /// assets. When all references are loaded, these should be superceeded
    /// by <see cref="LinkedToken"/>s.
    /// </summary>
    public class ParsedToken
    {
        public readonly ScriptingCommand CommandToExecute;
        public readonly List<LowercaseString> Arguments;

        public ParsedToken(ScriptingCommand commandToExecute, List<LowercaseString> arguments)
        {
            this.CommandToExecute = commandToExecute;
            this.Arguments = arguments;
        }
    }
}