namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the base concept of a ScriptingCommand.
    /// 
    /// A ScriptingCommand is a set of abstract instructions,
    /// but without context. It is aware of the context necessary for
    /// its own execution, as well as not knowing its parameters.
    /// 
    /// A ScriptingCommand that is provided parameters is
    /// a ParsedToken. 
    /// </summary>
    public abstract class ScriptingCommand
    {
    }
}