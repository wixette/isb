// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

namespace ISB.Runtime
{
    public sealed class BooleanValue : BaseValue
    {
        public BooleanValue(bool value)
        {
            this.Value = value;
        }

        public bool Value { get; private set; }

        public override string ToDisplayString() => this.Value ? "True" : "False";

        public override bool ToBoolean() => this.Value;

        public override decimal ToNumber() => this.Value ? 1 : 0;

        public static BooleanValue Parse(string s)
        {
            switch (s.Trim().ToLower())
            {
                case "true":
                    return new BooleanValue(true);
                case "false":
                default:
                    return new BooleanValue(false);
            }
        }
    }
}