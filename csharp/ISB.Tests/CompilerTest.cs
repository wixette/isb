using System.Collections.Generic;
using Xunit;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Runtime;
using ISB.Utilities;

namespace ISB.Tests
{
    public class CompilerTest
    {
        const string code1 = @"";
        const string assembly1 = @"";

        const string code2 = @"' comments";
        const string assembly2 = @"";

        const string code3 = @"a:
 b:
  c:";
        const string assembly3 = @"a:
    nop
b:
    nop
c:
    nop
";

        const string code4 = @"a:
goto a";
        const string assembly4 = @"a:
    br a
";

        const string code5 = @"sub a
endsub";
        const string assembly5 = @"    br __Program_0__
__Sub_a__:
    ret 0
__Program_0__:
    nop
";

        const string code6 = @"-1";
        const string assembly6 = @"    push 0
    push 1
    sub
";

        const string code7 = @"1-1";
        const string assembly7 = @"    push 1
    push 1
    sub
";

        const string code8 = @"a * b + -2";
        const string assembly8 = @"    load a
    load b
    mul
    push 0
    push 2
    sub
    add
";

        const string code9 = @"a > 1 and a < 3";
        const string assembly9 = @"    load a
    push 1
    gt
    br_if __Program_0__ __Program_2__
__Program_0__:
    load a
    push 3
    lt
    br_if __Program_1__ __Program_2__
__Program_1__:
    pushb True
    br __Program_3__
__Program_2__:
    pushb False
__Program_3__:
    nop
";

        const string code10 = @"a >= 3 or b <= 3";
        const string assembly10 = @"    load a
    push 3
    ge
    br_if __Program_1__ __Program_0__
__Program_0__:
    load b
    push 3
    le
    br_if __Program_1__ __Program_2__
__Program_1__:
    pushb True
    br __Program_3__
__Program_2__:
    pushb False
__Program_3__:
    nop
";

        const string code11 = @"a = 3";
        const string assembly11 = @"    push 3
    store a
";

        const string code12 = @"a = Math.Pi";
        const string assembly12 = @"    load_lib Math Pi
    store a
";

        const string code13 = @"Print(333)";
        const string assembly13 = @"    push 333
    call_lib BuiltIn Print
";

        const string code14 = @"3 * (4 - 5 * (2 / (2)))";
        const string assembly14 = @"    push 3
    push 4
    push 5
    push 2
    push 2
    div
    mul
    sub
    mul
";

        const string code15 = @"a[0]";
        const string assembly15 = @"    push 0
    load_arr a 1
";

        const string code16 = @"a[0][first]";
        const string assembly16 = @"    load first
    push 0
    load_arr a 2
";

        const string code17 = @"a[0][1][2][3] = 4";
        const string assembly17 = @"    push 4
    push 3
    push 2
    push 1
    push 0
    store_arr a 4
";

        const string code18 = @"a[identifier][""string""][123] = ""value""";
        const string assembly18 = @"    pushs ""value""
    push 123
    pushs ""string""
    load identifier
    store_arr a 3
";

        const string code19 = @"sub foo
endsub
foo()";
        const string assembly19 = @"    br __Program_0__
__Sub_foo__:
    ret 0
__Program_0__:
    call __Sub_foo__
";

        const string code20 = @"Math.Sin(y + 0.5)";
        const string assembly20 = @"    load y
    push 0.5
    add
    call_lib Math Sin
";

        const string code21 = @"If a = 3 Then
  b = 4
EndIf";
        const string assembly21 = @"    load a
    push 3
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    push 4
    store b
    br __Program_0__
__Program_2__:
    nop
__Program_0__:
    nop
";

        const string code22 = @"If a = 3 Then
  b = 4
ElseIf a = 4 Then
  b = 5
Else
  b = 6
EndIf";
        const string assembly22 = @"    load a
    push 3
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    push 4
    store b
    br __Program_0__
__Program_2__:
    load a
    push 4
    eq
    br_if __Program_3__ __Program_4__
__Program_3__:
    push 5
    store b
    br __Program_0__
__Program_4__:
    push 6
    store b
__Program_0__:
    nop
";

        const string code23 = @"While i < 10
  i = i + 1
EndWhile";
        const string assembly23 = @"__Program_0__:
    load i
    push 10
    lt
    br_if __Program_1__ __Program_2__
__Program_1__:
    load i
    push 1
    add
    store i
    br __Program_0__
__Program_2__:
    nop
";

        const string code24 = @"For i = 1 to 5
EndFor";
        const string assembly24 = @"    push 1
    store i
__Program_0__:
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
    load i
    push 5
    le
    br_if __Program_0__ __Program_1__
__Program_3__:
    load i
    push 5
    ge
    br_if __Program_0__ __Program_1__
__Program_1__:
    nop
";

        const string code25 = @"For i = 10 To 1 Step -1
  a = a + i
EndFor";
        const string assembly25 = @"    push 10
    store i
__Program_0__:
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
    load i
    push 1
    le
    br_if __Program_0__ __Program_1__
__Program_3__:
    load i
    push 1
    ge
    br_if __Program_0__ __Program_1__
__Program_1__:
    nop
";

        const string code26 = @"If a mod 2 = 0 or a mod 3 = 0 then
    x = 0
endif";
        const string assembly26 = @"    load a
    push 2
    mod
    push 0
    eq
    br_if __Program_4__ __Program_3__
__Program_3__:
    load a
    push 3
    mod
    push 0
    eq
    br_if __Program_4__ __Program_5__
__Program_4__:
    pushb True
    br __Program_6__
__Program_5__:
    pushb False
__Program_6__:
    br_if __Program_1__ __Program_2__
__Program_1__:
    push 0
    store x
    br __Program_0__
__Program_2__:
    nop
__Program_0__:
    nop
";

        const string code27 = @"If a = 3 Then
EndIf";
        const string assembly27 = @"    load a
    push 3
    eq
    br_if __Program_1__ __Program_2__
__Program_1__:
    br __Program_0__
__Program_2__:
    nop
__Program_0__:
    nop
";

        const string code28 = @"not (3 > 4)";
        const string assembly28 = @"    push 3
    push 4
    gt
    br_if __Program_0__ __Program_1__
__Program_0__:
    pushb False
    br __Program_2__
__Program_1__:
    pushb True
__Program_2__:
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
        [InlineData (code26, assembly26)]
        [InlineData (code27, assembly27)]
        [InlineData (code28, assembly28)]
        public void TestFormatAndParse(string code, string assembly)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(code, diagnostics);
            Assert.Empty(diagnostics.Contents);
            SyntaxNode tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Environment env = new Environment(null, null);
            Assembly instructions = Compiler.Compile(tree, env, "Program", diagnostics, out _, out _);
            Assert.Empty(diagnostics.Contents);

            // System.Console.WriteLine(code);
            // System.Console.WriteLine(compiler.Instructions.ToTextFormat());

            Assert.Equal(assembly, instructions.ToTextFormat(), ignoreLineEndingDifferences: true);
        }

        const string errInput1 = @"a:
a:";
        const string errInput2 = @"goto a";

        const string errInput3 = @"sub a
endsub
sub a
endsub";
        const string errInput4 = @"Math.Pi.x = 3";

        const string errInput5 = @"foo()";

        [Theory]
        [InlineData(errInput1, new Diagnostic.ErrorCode[] {Diagnostic.ErrorCode.TwoLabelsWithTheSameName})]
        [InlineData(errInput2, new Diagnostic.ErrorCode[] {Diagnostic.ErrorCode.GoToUndefinedLabel})]
        [InlineData(errInput3, new Diagnostic.ErrorCode[] {Diagnostic.ErrorCode.TwoSubModulesWithTheSameName})]
        [InlineData(errInput4, new Diagnostic.ErrorCode[] {Diagnostic.ErrorCode.UnsupportedDotBaseExpression})]
        [InlineData(errInput5, new Diagnostic.ErrorCode[] {Diagnostic.ErrorCode.UnsupportedInvocationBaseExpression})]
        public void TestErrorCases(string errInput, Diagnostic.ErrorCode[] errDiagnostics)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(errInput, diagnostics);
            Assert.Empty(diagnostics.Contents);
            SyntaxNode tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Environment env = new Environment(null, null);
            Assembly instructions = Compiler.Compile(tree, env, "Program", diagnostics, out _, out _);
            Assert.Equal(errDiagnostics.Length, diagnostics.Contents.Count);
            for (int i = 0; i < errDiagnostics.Length; i++)
            {
                Assert.Equal(errDiagnostics[i], diagnostics.Contents[i].Code);
            }
        }

        [Fact]
        public void TestSourceMap()
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(code22, diagnostics);
            Assert.Empty(diagnostics.Contents);
            SyntaxNode tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Environment env = new Environment(null, null);
            Assembly instructions = Compiler.Compile(tree, env, "Program", diagnostics, out _, out _);
            Assert.Empty(diagnostics.Contents);

            // System.Console.WriteLine(code22);
            // System.Console.WriteLine(compiler.Instructions.ToTextFormat());

            TextRange[] expectedSourceMap = new TextRange[] {
                ((0, 3), (0, 3)),
                ((0, 7), (0, 7)),
                ((0, 3), (0, 7)),
                ((0, 0), (1, 6)),
                ((1, 6), (1, 6)),
                ((1, 2), (1, 6)),
                ((0, 0), (1, 6)),
                ((2, 7), (2, 7)),
                ((2, 11), (2, 11)),
                ((2, 7), (2, 11)),
                ((2, 0), (3, 6)),
                ((3, 6), (3, 6)),
                ((3, 2), (3, 6)),
                ((2, 0), (3, 6)),
                ((5, 6), (5, 6)),
                ((5, 2), (5, 6)),
                ((0, 0), (6, 4))
            };
            for (int i = 0; i < instructions.Count; i++)
            {
                Assert.Equal(expectedSourceMap[i], instructions.LookupSourceMap(i));
            }
        }

        void MergeSymbolSet(HashSet<string> to, HashSet<string> from)
        {
            foreach (var k in from)
                to.Add(k);
        }

        [Fact]
        public void TestIncrementalCompilation()
        {
            const string codeSeg1 = @"a = 10
label1:";
            const string codeSeg2 = @"if a < 10 then
  goto label1
endif";
            const string codeSeg3 = @"label2:
3 = 4";
            const string codeSeg4 = @"label2:
if a < 20 then
  goto label1
endif";
            const string codeSeg5 = @"label1:";

            Assembly instructions = new Assembly();
            DiagnosticBag diagnostics = new DiagnosticBag();
            Environment env = new Environment(null, null);

            var tokens = Scanner.Scan(codeSeg1, diagnostics);
            Assert.Empty(diagnostics.Contents);
            SyntaxNode tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);

            Assembly assembly1 = Compiler.Compile(tree, env, "Program", diagnostics,
                out HashSet<string> newLabels1, out HashSet<string> newSubNames1);
            Assert.Empty(diagnostics.Contents);
            Assert.Single(newLabels1);
            Assert.Empty(newSubNames1);
            instructions.Append(assembly1, 0);
            MergeSymbolSet(env.CompileTimeLabels, newLabels1);
            MergeSymbolSet(env.CompileTimeSubNames, newSubNames1);
            Assert.Single(env.CompileTimeLabels);
            Assert.Empty(env.CompileTimeSubNames);
            Assert.Equal(@"    push 10
    store a
label1:
    nop
", instructions.ToTextFormat(), ignoreLineEndingDifferences: true);

            tokens = Scanner.Scan(codeSeg2, diagnostics);
            Assert.Empty(diagnostics.Contents);
            tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Assembly assembly2 = Compiler.Compile(tree, env, "Program", diagnostics,
                out HashSet<string> newLabels2, out HashSet<string> newSubNames2);
            Assert.Empty(diagnostics.Contents);
            Assert.Empty(newLabels2);
            Assert.Empty(newSubNames2);
            instructions.Append(assembly2, 2);
            MergeSymbolSet(env.CompileTimeLabels, newLabels2);
            MergeSymbolSet(env.CompileTimeSubNames, newSubNames2);
            Assert.Equal(@"    push 10
    store a
label1:
    nop
    load a
    push 10
    lt
    br_if __Program_1__ __Program_2__
__Program_1__:
    br label1
    br __Program_0__
__Program_2__:
    nop
__Program_0__:
    nop
", instructions.ToTextFormat(), ignoreLineEndingDifferences: true);

            tokens = Scanner.Scan(codeSeg3, diagnostics);
            Assert.Empty(diagnostics.Contents);
            tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Assembly assembly3 = Compiler.Compile(tree, env, "Program", diagnostics,
                out HashSet<string> newLabels3, out HashSet<string> newSubNames3);
            Assert.Single(diagnostics.Contents);
            Assert.Equal(Diagnostic.ErrorCode.ExpectedALeftValue, diagnostics.Contents[0].Code);
            // No merging here due to the error found.
            diagnostics.Reset();

            tokens = Scanner.Scan(codeSeg4, diagnostics);
            Assert.Empty(diagnostics.Contents);
            tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Assembly assembly4 = Compiler.Compile(tree, env, "Program", diagnostics,
                out HashSet<string> newLabels4, out HashSet<string> newSubNames4);
            Assert.Empty(diagnostics.Contents);
            Assert.Single(newLabels4);
            Assert.Empty(newSubNames4);
            instructions.Append(assembly4, 5);
            MergeSymbolSet(env.CompileTimeLabels, newLabels4);
            MergeSymbolSet(env.CompileTimeSubNames, newSubNames4);
            Assert.Equal(2, env.CompileTimeLabels.Count);
            Assert.Empty(env.CompileTimeSubNames);
            Assert.Equal(@"    push 10
    store a
label1:
    nop
    load a
    push 10
    lt
    br_if __Program_1__ __Program_2__
__Program_1__:
    br label1
    br __Program_0__
__Program_2__:
    nop
__Program_0__:
    nop
label2:
    load a
    push 20
    lt
    br_if __Program_4__ __Program_5__
__Program_4__:
    br label1
    br __Program_3__
__Program_5__:
    nop
__Program_3__:
    nop
", instructions.ToTextFormat(), ignoreLineEndingDifferences: true);

            tokens = Scanner.Scan(codeSeg5, diagnostics);
            Assert.Empty(diagnostics.Contents);
            tree = Parser.Parse(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Assembly assembly5 = Compiler.Compile(tree, env, "Program", diagnostics,
                out HashSet<string> newLabels5, out HashSet<string> newSubNames5);
            Assert.Single(diagnostics.Contents);
            Assert.Equal(Diagnostic.ErrorCode.TwoLabelsWithTheSameName, diagnostics.Contents[0].Code);
            // No merging here due to the error found.
            diagnostics.Reset();

            // Checks the merged source map.
            TextRange[] expectedSourceMap = new TextRange[] {
                ((0, 4), (0, 5)),
                ((0, 0), (0, 5)),
                ((1, 0), (1, 6)),
                ((2, 3), (2, 3)),
                ((2, 7), (2, 8)),
                ((2, 3), (2, 8)),
                ((2, 0), (3, 12)),
                ((3, 2), (3, 12)),
                ((2, 0), (3, 12)),
                ((2, 0), (3, 12)),
                ((2, 0), (4, 4)),
                ((6, 3), (6, 3)),
                ((6, 7), (6, 8)),
                ((6, 3), (6, 8)),
                ((6, 0), (7, 12)),
                ((7, 2), (7, 12)),
                ((6, 0), (7, 12)),
                ((6, 0), (7, 12)),
                ((6, 0), (8, 4))
            };
            for (int i = 0; i < instructions.Count; i++)
            {
                Assert.Equal(expectedSourceMap[i], instructions.LookupSourceMap(i));
            }
        }
    }
}
