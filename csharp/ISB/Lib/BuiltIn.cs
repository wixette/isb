using System;
using ISB.Runtime;
using ISB.UnityIntegration;
using ISB.Utilities;

namespace ISB.Lib
{
    [Preserve]
    public class BuiltIn
    {
        [Doc("Outputs number or string to the console, followed by a new line character.")]
        [Preserve]
        public void Print(BaseValue v)
        {
            Console.WriteLine(v.ToDisplayString());
        }

        [Doc("Outputs number or string to the console.")]
        [Preserve]
        public void Write(BaseValue v)
        {
            Console.Write(v.ToDisplayString());
        }
    }
}
