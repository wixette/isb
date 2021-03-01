using System;
using System.IO;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Shell
{
    internal sealed class Compiler
    {
        public static bool CompileToTextFormat(string fileName, string code, TextWriter output, TextWriter err)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(code, diagnostics);
            SyntaxNode tree = Parser.Parse(tokens, diagnostics);
            if (diagnostics.Contents.Count > 0)
            {
                ErrorReport.Report(code, diagnostics, err);
                return false;
            }

            ISB.Runtime.Environment environment = new ISB.Runtime.Environment();
            AssemblyGenerator generator =
                new AssemblyGenerator(environment, "Program", diagnostics);
            generator.Generate(tree);
            if (diagnostics.Contents.Count > 0)
            {
                ErrorReport.Report(code, diagnostics, err);
                return false;
            }

            string commentLine = ';' + new String('-', 99);
            output.WriteLine(commentLine);
            output.WriteLine($"; The ISB Assembly code generated from {fileName}");
            output.WriteLine($"; The code can be parsed and run by the shell tool of ISB (Interactive Small Basic).");
            output.WriteLine($"; See https://github.com/wixette/isb for more details.");
            output.WriteLine(commentLine);
            output.WriteLine(generator.Instructions.ToTextFormat());
            return true;
        }
    }
}