using System;
using ISB.Runtime;

namespace ISB.Lib
{
    public class Math
    {
        public Math()
        {
            this.Pi = new NumberValue(3.1415926535897931m);
            this.E = new NumberValue(2.7182818284590451m);
        }

        public NumberValue Pi { get; private init; }

        public NumberValue E { get; private init; }

        public NumberValue Abs(NumberValue x) {
            double ret = System.Math.Abs((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Acos(NumberValue x) {
            double ret = System.Math.Acos((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Asin(NumberValue x) {
            double ret = System.Math.Asin((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Atan(NumberValue x) {
            double ret = System.Math.Atan((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Ceiling(NumberValue x) {
            double ret = System.Math.Ceiling((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Cos(NumberValue x) {
            double ret = System.Math.Cos((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Exp(NumberValue x) {
            double ret = System.Math.Exp((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Floor(NumberValue x) {
            double ret = System.Math.Floor((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Log(NumberValue x) {
            double ret = System.Math.Log((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Log10(NumberValue x) {
            double ret = System.Math.Log10((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Max(NumberValue x, NumberValue y) {
            return x.ToNumber() > y.ToNumber() ? x : y;
        }

        public NumberValue Min(NumberValue x, NumberValue y) {
            return x.ToNumber() < y.ToNumber() ? x : y;
        }

        public NumberValue Pow(NumberValue x, NumberValue y) {
            double ret = System.Math.Pow((double)x.ToNumber(), (double)y.ToNumber());
            return new NumberValue((decimal)ret);
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

        public NumberValue Round(NumberValue x) {
            double ret = System.Math.Round((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Sign(NumberValue x) {
            double ret = System.Math.Sign((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Sin(NumberValue x) {
            double ret = System.Math.Sin((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Sqrt(NumberValue x) {
            double ret = System.Math.Sqrt((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }

        public NumberValue Tan(NumberValue x) {
            double ret = System.Math.Tan((double)x.ToNumber());
            return new NumberValue((decimal)ret);
        }
    }
}