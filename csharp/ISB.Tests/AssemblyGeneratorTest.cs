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
        private const string assembly5 = @"    br __Program_1__
__Program_0__:
    nop
    ret
__Program_1__:
    nop
";

        private const string code6 = @"-1";
        private const string assembly6 = @"    push 0
    push 1
    sub
";

        private const string code7 = @"1-1";
        private const string assembly7 = @"    push 1
    push 1
    sub
";

        private const string code8 = @"a * b + -2";
        private const string assembly8 = @"    load a
    load b
    mul
    push 0
    push 2
    sub
    add
";

        private const string code9 = @"a = 1 and b = 2";
        private const string assembly9 = @"    load a
    push 1
    eq
    br_if __Program_0__ __Program_2__
__Program_0__:
    nop
    load b
    push 2
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    nop
    push 1
    br __Program_3__
__Program_2__:
    nop
    push 0
__Program_3__:
    nop
";

        private const string code10 = @"a = 1 or b = 2";
        private const string assembly10 = @"    load a
    push 1
    eq
    br_if __Program_1__ __Program_0__
__Program_0__:
    nop
    load b
    push 2
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    nop
    push 1
    br __Program_3__
__Program_2__:
    nop
    push 0
__Program_3__:
    nop
";

        [Theory]
        [InlineData (code1, assembly1)]
        [InlineData (code2, assembly2)]
        [InlineData (code3, assembly3)]
        [InlineData (code4, assembly4)]
        [InlineData (code5, assembly5)]
        [InlineData (code6, assembly6)]
        [InlineData (code7, assembly7)]
        // [InlineData (code8, assembly8)]
        // [InlineData (code9, assembly9)]
        public void TestFormatAndParse(string code, string assembly)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(code, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Environment environment = new Environment();
            AssemblyGenerator generator =
                new AssemblyGenerator(environment, "Program", diagnostics);
            generator.Generate(parser.SyntaxTree);
            Assert.Empty(diagnostics.Contents);
            Assert.Equal(assembly, generator.Instructions.ToTextFormat());

            System.Console.WriteLine(SyntaxTreeDumper.Dump(parser.SyntaxTree));
            System.Console.WriteLine(generator.Instructions.ToTextFormat());
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
                new AssemblyGenerator(environment, "Program", diagnostics);
            generator.Generate(parser.SyntaxTree);
            Assert.Equal(errDiagnostics.Length, diagnostics.Contents.Count);
            for (int i = 0; i < errDiagnostics.Length; i++)
            {
                Assert.Equal(errDiagnostics[i], diagnostics.Contents[i].Code);
            }
        }
    }
}