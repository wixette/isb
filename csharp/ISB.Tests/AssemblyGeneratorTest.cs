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
    ret 0
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

        private const string code9 = @"a > 1 and a < 3";
        private const string assembly9 = @"    load a
    push 1
    gt
    br_if __Program_0__ __Program_2__
__Program_0__:
    nop
    load a
    push 3
    lt
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

        private const string code10 = @"a >= 3 or b <= 3";
        private const string assembly10 = @"    load a
    push 3
    ge
    br_if __Program_1__ __Program_0__
__Program_0__:
    nop
    load b
    push 3
    le
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

        private const string code11 = @"a = 3";
        private const string assembly11 = @"    push 3
    store a
";

        private const string code12 = @"LibA.PropertyFoo = 3";
        private const string assembly12 = @"    push 3
    store_lib LibA PropertyFoo
";

        private const string code13 = @"a = LibA.PropertyFoo";
        private const string assembly13 = @"    load_lib LibA PropertyFoo
    store a
";

        private const string code14 = @"3 * (4 - 5 * (2 / (2)))";
        private const string assembly14 = @"    push 3
    push 4
    push 5
    push 2
    push 2
    div
    mul
    sub
    mul
";

        private const string code15 = @"a[0]";
        private const string assembly15 = @"    push 0
    load_arr a 1
";

        private const string code16 = @"a[0][first]";
        private const string assembly16 = @"    load first
    push 0
    load_arr a 2
";

        private const string code17 = @"a[0][1][2][3] = 4";
        private const string assembly17 = @"    push 4
    push 3
    push 2
    push 1
    push 0
    store_arr a 4
";

        private const string code18 = @"a[identifier][""string""][123] = ""value""";
        private const string assembly18 = @"    pushs ""value""
    push 123
    pushs ""string""
    load identifier
    store_arr a 3
";

        private const string code19 = @"sub foo
endsub
foo()";
        private const string assembly19 = @"    br __Program_1__
__Program_0__:
    nop
    ret 0
__Program_1__:
    nop
    call foo
";

        private const string code20 = @"Math.Pow(x, y + 0.5)";
        private const string assembly20 = @"    load y
    push 0.5
    add
    load x
    call_lib Math Pow
";

        private const string code21 = @"If a = 3 Then
  b = 4
EndIf";
        private const string assembly21 = @"    load a
    push 3
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    nop
    push 4
    store b
    br __Program_0__
__Program_2__:
    nop
__Program_0__:
    nop
";

        private const string code22 = @"If a = 3 Then
  b = 4
ElseIf a = 4 Then
  b = 5
Else
  b = 6
EndIf";
        private const string assembly22 = @"    load a
    push 3
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    nop
    push 4
    store b
    br __Program_0__
__Program_2__:
    nop
    load a
    push 4
    eq
    br_if __Program_3__ __Program_4__
__Program_3__:
    nop
    push 5
    store b
    br __Program_0__
__Program_4__:
    nop
    push 6
    store b
__Program_0__:
    nop
";

        private const string code23 = @"While i < 10
  i = i + 1
EndWhile";
        private const string assembly23 = @"__Program_0__:
    nop
    load i
    push 10
    lt
    br_if __Program_1__ __Program_2__
__Program_1__:
    nop
    load i
    push 1
    add
    store i
    br __Program_0__
__Program_2__:
    nop
";

        private const string code24 = @"For i = 1 to 5
EndFor";
        private const string assembly24 = @"    push 1
    store i
__Program_0__:
    nop
    push 1
    set 0
    load i
    get 0
    add
    store i
    get 0
    push 0
    ge
    br_if __Program_2__ __Program_3__
__Program_2__:
    nop
    load i
    push 5
    le
    br_if __Program_0__ __Program_1__
__Program_3__:
    nop
    load i
    push 5
    ge
    br_if __Program_0__ __Program_1__
__Program_1__:
    nop
";

        private const string code25 = @"For i = 10 To 1 Step -1
  a = a + i
EndFor";
        private const string assembly25 = @"    push 10
    store i
__Program_0__:
    nop
    load a
    load i
    add
    store a
    push 0
    push 1
    sub
    set 0
    load i
    get 0
    add
    store i
    get 0
    push 0
    ge
    br_if __Program_2__ __Program_3__
__Program_2__:
    nop
    load i
    push 1
    le
    br_if __Program_0__ __Program_1__
__Program_3__:
    nop
    load i
    push 1
    ge
    br_if __Program_0__ __Program_1__
__Program_1__:
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
        [InlineData (code8, assembly8)]
        [InlineData (code9, assembly9)]
        [InlineData (code10, assembly10)]
        [InlineData (code11, assembly11)]
        [InlineData (code12, assembly12)]
        [InlineData (code13, assembly13)]
        [InlineData (code14, assembly14)]
        [InlineData (code15, assembly15)]
        [InlineData (code16, assembly16)]
        [InlineData (code17, assembly17)]
        [InlineData (code18, assembly18)]
        [InlineData (code19, assembly19)]
        [InlineData (code20, assembly20)]
        [InlineData (code21, assembly21)]
        [InlineData (code22, assembly22)]
        [InlineData (code23, assembly23)]
        [InlineData (code24, assembly24)]
        [InlineData (code25, assembly25)]
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

            System.Console.WriteLine(code);
            System.Console.WriteLine(generator.Instructions.ToTextFormat());

            Assert.Equal(assembly, generator.Instructions.ToTextFormat());
        }

        const string errInput1 = @"a:
a:";
        const string errInput2 = @"goto a";

        const string errInput3 = @"sub a
endsub
sub a
endsub";
        const string errInput4 = @"LibA.PropertyFoo.x = 3";

        const string errInput5 = @"foo()";

        [Theory]
        [InlineData(errInput1, new DiagnosticCode[] {DiagnosticCode.TwoLabelsWithTheSameName})]
        [InlineData(errInput2, new DiagnosticCode[] {DiagnosticCode.GoToUndefinedLabel})]
        [InlineData(errInput3, new DiagnosticCode[] {DiagnosticCode.TwoSubModulesWithTheSameName})]
        [InlineData(errInput4, new DiagnosticCode[] {DiagnosticCode.UnsupportedDotBaseExpression})]
        [InlineData(errInput5, new DiagnosticCode[] {DiagnosticCode.UnsupportedInvocationBaseExpression})]
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