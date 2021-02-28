using System.IO;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Shell
{
    internal class Compiler
    {
        public static bool Compile(string code, TextWriter output, TextWriter err)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(code, diagnostics);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            if (diagnostics.Contents.Count > 0)
            {
                err.WriteLine("Errors found in scanning/parsing");
                return false;
            }
            Environment environment = new Environment();
            AssemblyGenerator generator =
                new AssemblyGenerator(environment, "Program", diagnostics);
            generator.Generate(parser.SyntaxTree);
            if (diagnostics.Contents.Count > 0)
            {
                err.WriteLine("Errors found in IR generating");
            }
            output.WriteLine(generator.Instructions.ToTextFormat());
            return true;
        }
    }
}