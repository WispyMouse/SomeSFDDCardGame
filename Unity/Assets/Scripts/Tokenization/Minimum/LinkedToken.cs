namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// The combination of <see cref="ParsedToken"/>,
    /// database access, and saturation of arguments.
    /// 
    /// This is a mostly processed and prepared to be Executed token.
    /// </summary>
    public abstract class LinkedToken<T> : LinkedToken, ILinkedCommand<T> where T : ScriptingCommand
    {
        public T CastCommandToExecute;

        public LinkedToken(ScriptingCommand commandToExecute, List<LowercaseString> arguments) : base(commandToExecute, arguments)
        {
        }

        public override ScriptingCommand GetScriptingCommand()
        {
            return this.CommandToExecute;
        }
    }

    /// <summary>
    /// Base class of the LinkedToken. Allows it to be more
    /// easily referenced in a C# fashion, and is technically
    /// a possible resulting Token layout.
    /// </summary>
    public abstract class LinkedToken : ParsedToken
    {
        public LinkedToken(ScriptingCommand commandToExecute, List<LowercaseString> arguments) : base(commandToExecute, arguments)
        {
        }

        public abstract ScriptingCommand GetScriptingCommand();
    }
}