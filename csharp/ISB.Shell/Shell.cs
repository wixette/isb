using System;
using System.Diagnostics;
using System.Reflection;

namespace ISB
{
    class Shell
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine($"{fvi.ProductName}, v{fvi.ProductVersion}, {fvi.LegalCopyright}");
            Console.WriteLine("Welcome");
        }
    }
}
