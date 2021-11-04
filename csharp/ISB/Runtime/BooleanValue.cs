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

        public override string ToDisplayString() =>
            this.Value ? Instruction.TrueLiteral : Instruction.FalseLiteral;

        public override bool ToBoolean() => this.Value;

        public override decimal ToNumber() => this.Value ? 1 : 0;

        // Note: this parser is only used to parse boolean operands in assembly instructions such as
        // PUSHB. The implicit value-to-boolean conversion must not rely on this parser since the
        // rules are different.
        public static BooleanValue ParseBooleanOperand(string s) =>
            s.ToLower() == Instruction.TrueLiteral.ToLower() ?
                new BooleanValue(true) : new BooleanValue(false);

        public override object Clone()
        {
            return new BooleanValue(this.Value);
        }
    }
}
