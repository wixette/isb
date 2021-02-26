// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Diagnostics;

namespace ISB.Runtime
{
    public sealed class StringValue : BaseValue
    {
        public StringValue(string value)
        {
            Debug.Assert(value != null, "Value should never be null.");
            this.Value = value;
        }

        public string Value { get; private set; }

        public static StringValue Empty => new StringValue(string.Empty);

        public override string ToDisplayString() => this.Value;

        public override bool ToBoolean() => this.Value.Length == 0 ? false : true;

        public override decimal ToNumber()
        {
            return NumberValue.Parse(this.Value).Value;
        }

        public static StringValue ParseEscaped(string escapedString)
        {
            return new StringValue(Unescape(escapedString));
        }

        public static string Escape(string s)
        {
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }

        public static string Unescape(string s)
        {
            return s
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }
    }
}