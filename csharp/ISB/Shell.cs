using System;
using System.Diagnostics;
using System.Reflection;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Properties;
using ISB.Utilities;

namespace ISB
{
    class Shell
    {
        static void Main(string[] args)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Console.WriteLine($"{fvi.ProductName}, v{fvi.ProductVersion}, {fvi.LegalCopyright}");
            Console.WriteLine(Resources.Welcome);

            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner("a = \"hello\"", diagnostics);
            Parser parser = new Parser(scanner.Tokens, diagnostics);

            string dump = SyntaxTreeDumper.Dump(parser.SyntaxTree);
            Console.WriteLine(dump);
        }
    }
}
