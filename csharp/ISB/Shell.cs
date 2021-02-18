using System;
using ISB.Scanner;

namespace ISB
{
    class Shell
    {
        static void Main(string[] args)
        {
            TextPosition pos = new TextPosition(0, 0);
            Console.WriteLine(pos.ToDisplayString());
        }
    }
}
