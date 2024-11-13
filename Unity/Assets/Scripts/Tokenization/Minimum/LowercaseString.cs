namespace SpaceDeck.Tokenization.Minimum
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A string that is promising that it is in all lowercase.
    /// 
    /// All ids and actions are done in lowercase. This promises
    /// that designers don't need to fuss about capitalization.
    /// </summary>
    public struct LowercaseString : IEquatable<string>
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

        public static implicit operator LowercaseString(string text)
        {
            return new LowercaseString(text);
        }

        public static List<LowercaseString> FromArray(string[] text)
        {
            List<LowercaseString> strings = new List<LowercaseString>();

            foreach (string textItem in text)
            {
                strings.Add(new LowercaseString(textItem));
            }

            return strings;
        }

        public bool Equals(string other)
        {
            return this.Value.Equals(other.ToLower());
        }
    }
}