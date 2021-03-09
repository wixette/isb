using System;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Lib
{
    public class BuiltIn
    {
        [Doc("Outputs number or string to the console, followed by a new line character.")]
        public void Print(BaseValue v)
        {
            Console.WriteLine(v.ToDisplayString());
        }

        [Doc("Outputs number or string to the console.")]
        public void Write(BaseValue v)
        {
            Console.Write(v.ToDisplayString());
        }
    }
}