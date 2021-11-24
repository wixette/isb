// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Globalization;

namespace ISB.Runtime
{
    public sealed class NumberValue : BaseValue
    {
        /*
         * Note: We use a a number style slightly different from the default 'NumberStyles.Number'
         * because for parity with SBD, we don't allow thousands separators.
         */
        private const NumberStyles NumberStyle =
            NumberStyles.Integer | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint;

        public NumberValue(decimal value)
        {
            this.Value = value;
        }

        public decimal Value { get; private set; }

        public override string ToDisplayString() => this.Value.ToString(CultureInfo.CurrentCulture);

        public override bool ToBoolean() => this.Value == 0 ? false : true;

        public override decimal ToNumber() => this.Value;

        public static bool TryParse(string s, out NumberValue numberValue) {
            if (Decimal.TryParse(s.Trim(), NumberStyle,
                NumberFormatInfo.CurrentInfo, out decimal decimalResult))
            {
                numberValue = new NumberValue(decimalResult);
                return true;
            }
            else
            {
                numberValue = null;
                return false;
            }
        }

        public static NumberValue Parse(string s)
        {
            if (NumberValue.TryParse(s.Trim(), out NumberValue numberValue))
            {
                return numberValue;
            }
            else
            {
                return new NumberValue(0);
            }
        }

        public override object Clone()
        {
            return new NumberValue(this.Value);
        }
    }
}
