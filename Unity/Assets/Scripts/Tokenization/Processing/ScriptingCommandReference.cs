namespace SpaceDeck.Tokenization.Processing
{
    using SpaceDeck.Tokenization.Minimum;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A lookup table of all <see cref="ScriptingCommand"/>.
    /// 
    /// This is where ScriptingCommands should be registered, and where you can
    /// attempt to look them up.
    /// </summary>
    public static class ScriptingCommandReference
    {
        /// <summary>
        /// Internally maintained lookup of ScriptingCommands.
        /// </summary>
        private static Dictionary<LowercaseString, ScriptingCommand> s_IdentifierToScriptingCommand { get; set; } = new Dictionary<LowercaseString, ScriptingCommand>();

        /// <summary>
        /// Registers a ScriptingCommand, such that it could be recognized by its
        /// <see cref="ScriptingCommand.Identifier"/> through
        /// <see cref="TryGetScriptingCommandByIdentifier(LowercaseString, out ScriptingCommand)"/>.
        /// </summary>
        /// <param name="toRegister">The ScriptingCommand to register.</param>
        public static void RegisterScriptingCommand(ScriptingCommand toRegister)
        {
            LowercaseString toRegisterIdentifier = toRegister.Identifier;

            if (s_IdentifierToScriptingCommand.ContainsKey(toRegisterIdentifier))
            {
                // TODO: What do we do in this situation?
                // Likely need to log this.
                return;
            }

            s_IdentifierToScriptingCommand.Add(toRegisterIdentifier, toRegister);
        }

        /// <summary>
        /// Attempts to find a matching ScriptingCommand by its identifier.
        /// </summary>
        /// <param name="identifier">The identifier to look up by.</param>
        /// <param name="foundCommand">
        /// An out parameter containing the associated command.
        /// Should not be used if this function returns false.
        /// </param>
        /// <returns>True if a command is found. False otherwise.</returns>
        public static bool TryGetScriptingCommandByIdentifier(LowercaseString identifier, out ScriptingCommand foundCommand)
        {
            if (s_IdentifierToScriptingCommand.TryGetValue(identifier, out foundCommand))
            {
                return true;
            }

            return false;
        }
    }
}