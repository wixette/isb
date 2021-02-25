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
        private const string assembly1 = @"";

        private const string code2 = @"' comments";
        private const string assembly2 = @"";

        private const string code3 = @"a:
 b:
  c:";
        private const string assembly3 = @"a:
    nop
b:
    nop
c:
    nop
";

        private const string code4 = @"a:
goto a";
        private const string assembly4 = @"a:
    nop
    br a
";

        private const string code5 = @"sub a
endsub";
        private const string assembly5 = @"    br __Program_endsub_a__
__Program_sub_a__:
    nop
__Program_endsub_a__:
    nop
";

        [Theory]
        [InlineData (code1, assembly1)]
        [InlineData (code2, assembly2)]
        [InlineData (code3, assembly3)]
        [InlineData (code4, assembly4)]
        [InlineData (code5, assembly5)]
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

        const string errInput3 = @"sub a
endsub
sub a
endsub";

        [Theory]
        [InlineData(errInput1, new DiagnosticCode[] {DiagnosticCode.TwoLabelsWithTheSameName})]
        [InlineData(errInput2, new DiagnosticCode[] {DiagnosticCode.GoToUndefinedLabel})]
        [InlineData(errInput3, new DiagnosticCode[] {DiagnosticCode.TwoSubModulesWithTheSameName})]
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