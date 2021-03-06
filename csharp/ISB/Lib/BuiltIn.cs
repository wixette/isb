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
    }
}