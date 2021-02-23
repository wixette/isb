// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Diagnostics;
using System.Globalization;

namespace ISB.Runtime
{
    public sealed class StringValue : BaseValue
    {
        /*
         * Note: We use a a number style slightly different from the default 'NumberStyles.Number'
         * because for parity with SBD, we don't allow thousands separators.
         */
        private const NumberStyles NumberStyle =
            NumberStyles.Integer | NumberStyles.AllowTrailingSign | NumberStyles.AllowDecimalPoint;

        public StringValue(string value)
        {
            Debug.Assert(value != null, "Value should never be null.");
            this.Value = value;
        }

        public string Value { get; private set; }

        public static StringValue Empty => new StringValue(string.Empty);

        public static BaseValue Parse(string value)
        {
            switch (value.Trim().ToLower(CultureInfo.CurrentCulture))
            {
                case "true":
                    return new BooleanValue(true);
                case "false":
                    return new BooleanValue(false);
                case string other when
                        decimal.TryParse(other, NumberStyle, NumberFormatInfo.CurrentInfo, out decimal decimalResult):
                    return new NumberValue(decimalResult);
                default:
                    return new StringValue(value);
            }
        }

        public override string ToDisplayString() => this.Value;

        public override bool ToBoolean() => this.Value.Length == 0 ? false : true;

        public override decimal ToNumber()
        {
            var value = StringValue.Parse(this.Value);
            return value is NumberValue ? value.ToNumber() : 0;
        }
    }
}