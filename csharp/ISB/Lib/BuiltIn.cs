using System;
using ISB.Runtime;

namespace ISB.Lib
{
    public class BuiltIn
    {
        public void Print(BaseValue v)
        {
            Console.WriteLine(v.ToDisplayString());
        }

        public NumberValue Random()
        {
            return new NumberValue((decimal)new Random().NextDouble());
        }

        public NumberValue RandomInt(NumberValue max)
        {
            int maxValue = (int)System.Math.Floor((double)max.ToNumber());
            if (maxValue < 0) maxValue = 0;
            return new NumberValue(new Random().Next(maxValue));
        }
    }
}