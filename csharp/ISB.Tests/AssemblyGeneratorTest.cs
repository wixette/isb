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

        private const string code3 = @"a:
goto a";
        private const string assembly3 = @"a:
    nop
    br a
";

        [Theory]
        [InlineData (code1, assembly1)]
        [InlineData (code2, assembly2)]
        [InlineData (code3, assembly3)]
        public void TestFormatAndParse(string code, string assembly)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(code, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);

            System.Console.WriteLine(SyntaxTreeDumper.Dump(parser.SyntaxTree));

            Environment environment = new Environment();
            AssemblyGenerator generator =
                new AssemblyGenerator(environment, parser.SyntaxTree, "Program", 0, diagnostics);
            Assert.Empty(diagnostics.Contents);
            System.Console.WriteLine(generator.AssemblyBlock.ToTextFormat());
            Assert.Equal(assembly, generator.AssemblyBlock.ToTextFormat());
        }

        const string errInput1 = @"a:
a:";
        const string errInput2 = @"goto a";

        [Theory]
        [InlineData(errInput1, new DiagnosticCode[] {DiagnosticCode.TwoLabelsWithTheSameName})]
        [InlineData(errInput2, new DiagnosticCode[] {DiagnosticCode.GoToUndefinedLabel})]
        public void TestErrorCases(string errInput, DiagnosticCode[] errDiagnostics)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(errInput, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Environment environment = new Environment();
            AssemblyGenerator generator =
                new AssemblyGenerator(environment, parser.SyntaxTree, "Program", 0, diagnostics);
            Assert.Equal(errDiagnostics.Length, diagnostics.Contents.Count);
            for (int i = 0; i < errDiagnostics.Length; i++)
            {
                Assert.Equal(errDiagnostics[i], diagnostics.Contents[i].Code);
            }
        }
    }
}