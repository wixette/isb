using System;
using ISB.Scanner;

namespace ISB
{
    class Shell
    {
        static void Main(string[] args)
        {
            System.Reflection.Assembly assembly =
                System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi =
                System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine($"{fvi.ProductName}, v{fvi.ProductVersion}, {fvi.LegalCopyright}");
        }
    }
}
