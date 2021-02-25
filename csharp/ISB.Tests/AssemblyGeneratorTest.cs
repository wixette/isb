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

        private const string code2 = @"a:
 b:
  c:";
        private const string assembly2 = @"a:
    nop
b:
    nop
c:
    nop
";

        [Theory]
        [InlineData (code1, assembly1)]
        [InlineData (code2, assembly2)]
        public void TestFormatAndParse(string code, string assembly)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(code, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);

            System.Console.WriteLine(SyntaxTreeDumper.Dump(parser.SyntaxTree));

            Environment environment = new Environment();
            AssemblyGenerator generator = new AssemblyGenerator(environment, parser.SyntaxTree, "Program");
            Assert.Empty(diagnostics.Contents);
            System.Console.WriteLine(generator.AssemblyBlock.ToTextFormat());
            Assert.Equal(assembly, generator.AssemblyBlock.ToTextFormat());
        }
    }
}