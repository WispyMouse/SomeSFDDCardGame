namespace SpaceDeck.Tokenization.Minimum
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A string that is promising that it is in all lowercase.
    /// 
    /// All ids and actions are done in lowercase. This promises
    /// that designers don't need to fuss about capitalization.
    /// </summary>
    public struct LowercaseString
    {
        public string Value;

        public LowercaseString(string value)
        {
            this.Value = value.ToLower();
        }

        public override string ToString()
        {
            return this.Value;
        }
    }
}