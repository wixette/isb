using System;
using System.Collections.Generic;
using Xunit;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Tests
{
    public class AssemblyGeneratorTest
    {
        private const string code1 = @"";
        private const string assembly1 = @"    nop
";

        [Theory]
        [InlineData (code1, assembly1)]
        public void TestFormatAndParse(string code, string assembly)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(code, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Dictionary<string, int> labelDictionary = new Dictionary<string, int>();
            AssemblyGenerator generator = new AssemblyGenerator("main", parser.SyntaxTree, labelDictionary);
            Assert.Empty(diagnostics.Contents);
            Console.WriteLine(generator.AssemblyBlock.ToTextFormat());
            Assert.Equal(assembly, generator.AssemblyBlock.ToTextFormat());
        }
    }
}