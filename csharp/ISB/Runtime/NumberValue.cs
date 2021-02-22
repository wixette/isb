// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Globalization;

namespace ISB.Runtime
{
    public sealed class NumberValue : BaseValue
    {
        public NumberValue(decimal value)
        {
            this.Value = value;
        }

        public decimal Value { get; private set; }

        public override string ToDisplayString() => this.Value.ToString(CultureInfo.CurrentCulture);

        internal override bool ToBoolean() => this.Value == 0 ? false : true;

        internal override decimal ToNumber() => this.Value;
    }
}