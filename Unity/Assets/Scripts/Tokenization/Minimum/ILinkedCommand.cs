namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Indicates that this <see cref="LinkedToken"/> relates
    /// to this <see cref="ScriptingCommand"/>.
    /// 
    /// This is a mechanism for indicating saturated tokens to
    /// the tokens they're based off of. This allows for more
    /// complex saturated token parameters.
    /// </summary>
    public interface ILinkedCommand<T> where T : ScriptingCommand
    {
    }
}