using System;
using ISB.Runtime;

namespace ISB.Lib
{
    public class Math
    {
        public Math()
        {
            this.Pi = new NumberValue(3.1415926535897931m);
        }

        public NumberValue Pi { get; private init; }

        public NumberValue Sin(NumberValue x) {
            double value = System.Math.Sin(Convert.ToDouble(x.ToNumber()));
            return new NumberValue(Convert.ToDecimal(value));
        }

        public NumberValue Cos(NumberValue x) {
            double value = System.Math.Cos(Convert.ToDouble(x.ToNumber()));
            return new NumberValue(Convert.ToDecimal(value));
        }
    }
}