using Xunit;
using ISB.Parsing;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Tests
{
    public class ParserTest
    {
        const string code0 = @"";
        const string tree0 = @"EmptySyntax
";

        const string code1 = @"a";
        const string tree1 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    IdentifierExpressionSyntax: a
";

        const string code2 = @"3.14";
        const string tree2 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    NumberLiteralExpressionSyntax: 3.14
";

        const string code3 = @"""Hello""";
        const string tree3 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    StringLiteralExpressionSyntax: ""Hello""
";

        const string code4 = @"- 3.14 * r * r";
        const string tree4 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      BinaryOperatorExpressionSyntax
        UnaryOperatorExpressionSyntax
          PunctuationSyntax: -
          NumberLiteralExpressionSyntax: 3.14
        PunctuationSyntax: *
        IdentifierExpressionSyntax: r
      PunctuationSyntax: *
      IdentifierExpressionSyntax: r
";

        const string code5 = @"a = ""Hello""";
        const string tree5 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      IdentifierExpressionSyntax: a
      PunctuationSyntax: =
      StringLiteralExpressionSyntax: ""Hello""
";

        const string code6 = @"a = (6 + -3 * 2 - (7 - (x + (y))))";
        const string tree6 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      IdentifierExpressionSyntax: a
      PunctuationSyntax: =
      ParenthesisExpressionSyntax
        PunctuationSyntax: (
        BinaryOperatorExpressionSyntax
          NumberLiteralExpressionSyntax: 6
          PunctuationSyntax: +
          BinaryOperatorExpressionSyntax
            BinaryOperatorExpressionSyntax
              UnaryOperatorExpressionSyntax
                PunctuationSyntax: -
                NumberLiteralExpressionSyntax: 3
              PunctuationSyntax: *
              NumberLiteralExpressionSyntax: 2
            PunctuationSyntax: -
            ParenthesisExpressionSyntax
              PunctuationSyntax: (
              BinaryOperatorExpressionSyntax
                NumberLiteralExpressionSyntax: 7
                PunctuationSyntax: -
                ParenthesisExpressionSyntax
                  PunctuationSyntax: (
                  BinaryOperatorExpressionSyntax
                    IdentifierExpressionSyntax: x
                    PunctuationSyntax: +
                    ParenthesisExpressionSyntax
                      PunctuationSyntax: (
                      IdentifierExpressionSyntax: y
                      PunctuationSyntax: )
                  PunctuationSyntax: )
              PunctuationSyntax: )
        PunctuationSyntax: )
";

        const string code7 = @"Cos()";
        const string tree7 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    InvocationExpressionSyntax
      IdentifierExpressionSyntax: Cos
      PunctuationSyntax: (
      EmptySyntax
      PunctuationSyntax: )
";

        const string code8 = @"Math.Pow(x, y + 0.5)";
        const string tree8 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    InvocationExpressionSyntax
      ObjectAccessExpressionSyntax
        IdentifierExpressionSyntax: Math
        PunctuationSyntax: .
        IdentifierExpressionSyntax: Pow
      PunctuationSyntax: (
      ArgumentGroupSyntax
        ArgumentSyntax
          IdentifierExpressionSyntax: x
          PunctuationSyntax: ,
        ArgumentSyntax
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: y
            PunctuationSyntax: +
            NumberLiteralExpressionSyntax: 0.5
          EmptySyntax
      PunctuationSyntax: )
";

        const string code9 = @"a[0] = 3";
        const string tree9 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      ArrayAccessExpressionSyntax
        IdentifierExpressionSyntax: a
        PunctuationSyntax: [
        NumberLiteralExpressionSyntax: 0
        PunctuationSyntax: ]
      PunctuationSyntax: =
      NumberLiteralExpressionSyntax: 3
";

        const string code10 = @"If a = 3 Then
  b = 4
EndIf";
        const string tree10 = @"StatementBlockSyntax
  IfStatementSyntax
    IfPartSyntax
      KeywordSyntax: If
      BinaryOperatorExpressionSyntax
        IdentifierExpressionSyntax: a
        PunctuationSyntax: =
        NumberLiteralExpressionSyntax: 3
      KeywordSyntax: Then
      StatementBlockSyntax
        ExpressionStatementSyntax
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: b
            PunctuationSyntax: =
            NumberLiteralExpressionSyntax: 4
    EmptySyntax
    EmptySyntax
    KeywordSyntax: EndIf
";

        const string code11 = @"If a = 3 Then
  b = 4
ElseIf a = 4 Then
  b = 5
Else
  b = 6
EndIf";
        const string tree11 = @"StatementBlockSyntax
  IfStatementSyntax
    IfPartSyntax
      KeywordSyntax: If
      BinaryOperatorExpressionSyntax
        IdentifierExpressionSyntax: a
        PunctuationSyntax: =
        NumberLiteralExpressionSyntax: 3
      KeywordSyntax: Then
      StatementBlockSyntax
        ExpressionStatementSyntax
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: b
            PunctuationSyntax: =
            NumberLiteralExpressionSyntax: 4
    ElseIfPartGroupSyntax
      ElseIfPartSyntax
        KeywordSyntax: ElseIf
        BinaryOperatorExpressionSyntax
          IdentifierExpressionSyntax: a
          PunctuationSyntax: =
          NumberLiteralExpressionSyntax: 4
        KeywordSyntax: Then
        StatementBlockSyntax
          ExpressionStatementSyntax
            BinaryOperatorExpressionSyntax
              IdentifierExpressionSyntax: b
              PunctuationSyntax: =
              NumberLiteralExpressionSyntax: 5
    ElsePartSyntax
      KeywordSyntax: Else
      StatementBlockSyntax
        ExpressionStatementSyntax
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: b
            PunctuationSyntax: =
            NumberLiteralExpressionSyntax: 6
    KeywordSyntax: EndIf
";

        const string code12 = @"If a = 3 Then
EndIf";
        const string tree12 = @"StatementBlockSyntax
  IfStatementSyntax
    IfPartSyntax
      KeywordSyntax: If
      BinaryOperatorExpressionSyntax
        IdentifierExpressionSyntax: a
        PunctuationSyntax: =
        NumberLiteralExpressionSyntax: 3
      KeywordSyntax: Then
      EmptySyntax
    EmptySyntax
    EmptySyntax
    KeywordSyntax: EndIf
";

        const string code13 = @"For i = 1 to 5
EndFor";
        const string tree13 = @"StatementBlockSyntax
  ForStatementSyntax
    KeywordSyntax: For
    IdentifierExpressionSyntax: i
    KeywordSyntax: =
    NumberLiteralExpressionSyntax: 1
    KeywordSyntax: to
    NumberLiteralExpressionSyntax: 5
    EmptySyntax
    EmptySyntax
    KeywordSyntax: EndFor
";

        const string code14 = @"For i = 10 to 1 step -1
  j = i * 2
EndFor";
        const string tree14 = @"StatementBlockSyntax
  ForStatementSyntax
    KeywordSyntax: For
    IdentifierExpressionSyntax: i
    KeywordSyntax: =
    NumberLiteralExpressionSyntax: 10
    KeywordSyntax: to
    NumberLiteralExpressionSyntax: 1
    ForStepClauseSyntax
      KeywordSyntax: step
      UnaryOperatorExpressionSyntax
        PunctuationSyntax: -
        NumberLiteralExpressionSyntax: 1
    StatementBlockSyntax
      ExpressionStatementSyntax
        BinaryOperatorExpressionSyntax
          IdentifierExpressionSyntax: j
          PunctuationSyntax: =
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: i
            PunctuationSyntax: *
            NumberLiteralExpressionSyntax: 2
    KeywordSyntax: EndFor
";

        const string code15 = @"While i < 10
  i = i + 1
EndWhile";
        const string tree15 = @"StatementBlockSyntax
  WhileStatementSyntax
    KeywordSyntax: While
    BinaryOperatorExpressionSyntax
      IdentifierExpressionSyntax: i
      PunctuationSyntax: <
      NumberLiteralExpressionSyntax: 10
    StatementBlockSyntax
      ExpressionStatementSyntax
        BinaryOperatorExpressionSyntax
          IdentifierExpressionSyntax: i
          PunctuationSyntax: =
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: i
            PunctuationSyntax: +
            NumberLiteralExpressionSyntax: 1
    KeywordSyntax: EndWhile
";

        const string code16 = @"Label101:
Goto Label101";
        const string tree16 = @"StatementBlockSyntax
  LabelStatementSyntax
    IdentifierExpressionSyntax: Label101
    PunctuationSyntax: :
  GoToStatementSyntax
    KeywordSyntax: Goto
    IdentifierExpressionSyntax: Label101
";

        const string code17 = @"Sub Foo
  array[""key""] = (Math.Pi * ((((((x))))) - y)) + b[j]
EndSub";
        const string tree17 = @"StatementBlockSyntax
  SubModuleStatementSyntax
    KeywordSyntax: Sub
    IdentifierExpressionSyntax: Foo
    StatementBlockSyntax
      ExpressionStatementSyntax
        BinaryOperatorExpressionSyntax
          ArrayAccessExpressionSyntax
            IdentifierExpressionSyntax: array
            PunctuationSyntax: [
            StringLiteralExpressionSyntax: ""key""
            PunctuationSyntax: ]
          PunctuationSyntax: =
          BinaryOperatorExpressionSyntax
            ParenthesisExpressionSyntax
              PunctuationSyntax: (
              BinaryOperatorExpressionSyntax
                ObjectAccessExpressionSyntax
                  IdentifierExpressionSyntax: Math
                  PunctuationSyntax: .
                  IdentifierExpressionSyntax: Pi
                PunctuationSyntax: *
                ParenthesisExpressionSyntax
                  PunctuationSyntax: (
                  BinaryOperatorExpressionSyntax
                    ParenthesisExpressionSyntax
                      PunctuationSyntax: (
                      ParenthesisExpressionSyntax
                        PunctuationSyntax: (
                        ParenthesisExpressionSyntax
                          PunctuationSyntax: (
                          ParenthesisExpressionSyntax
                            PunctuationSyntax: (
                            ParenthesisExpressionSyntax
                              PunctuationSyntax: (
                              IdentifierExpressionSyntax: x
                              PunctuationSyntax: )
                            PunctuationSyntax: )
                          PunctuationSyntax: )
                        PunctuationSyntax: )
                      PunctuationSyntax: )
                    PunctuationSyntax: -
                    IdentifierExpressionSyntax: y
                  PunctuationSyntax: )
              PunctuationSyntax: )
            PunctuationSyntax: +
            ArrayAccessExpressionSyntax
              IdentifierExpressionSyntax: b
              PunctuationSyntax: [
              IdentifierExpressionSyntax: j
              PunctuationSyntax: ]
    KeywordSyntax: EndSub
";

        const string code18 = @"a[0][1] = 0";
        const string tree18 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      ArrayAccessExpressionSyntax
        ArrayAccessExpressionSyntax
          IdentifierExpressionSyntax: a
          PunctuationSyntax: [
          NumberLiteralExpressionSyntax: 0
          PunctuationSyntax: ]
        PunctuationSyntax: [
        NumberLiteralExpressionSyntax: 1
        PunctuationSyntax: ]
      PunctuationSyntax: =
      NumberLiteralExpressionSyntax: 0
";

        const string code19 = @"a[0][1][2] = 0";
        const string tree19 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      ArrayAccessExpressionSyntax
        ArrayAccessExpressionSyntax
          ArrayAccessExpressionSyntax
            IdentifierExpressionSyntax: a
            PunctuationSyntax: [
            NumberLiteralExpressionSyntax: 0
            PunctuationSyntax: ]
          PunctuationSyntax: [
          NumberLiteralExpressionSyntax: 1
          PunctuationSyntax: ]
        PunctuationSyntax: [
        NumberLiteralExpressionSyntax: 2
        PunctuationSyntax: ]
      PunctuationSyntax: =
      NumberLiteralExpressionSyntax: 0
";

const string code20 = @"a.b.c = 0";
        const string tree20 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    BinaryOperatorExpressionSyntax
      ObjectAccessExpressionSyntax
        ObjectAccessExpressionSyntax
          IdentifierExpressionSyntax: a
          PunctuationSyntax: .
          IdentifierExpressionSyntax: b
        PunctuationSyntax: .
        IdentifierExpressionSyntax: c
      PunctuationSyntax: =
      NumberLiteralExpressionSyntax: 0
";

const string code21 = @"a()()";
        const string tree21 = @"StatementBlockSyntax
  ExpressionStatementSyntax
    InvocationExpressionSyntax
      InvocationExpressionSyntax
        IdentifierExpressionSyntax: a
        PunctuationSyntax: (
        EmptySyntax
        PunctuationSyntax: )
      PunctuationSyntax: (
      EmptySyntax
      PunctuationSyntax: )
";

        [Theory]
        [InlineData(code0, tree0)]
        [InlineData(code1, tree1)]
        [InlineData(code2, tree2)]
        [InlineData(code3, tree3)]
        [InlineData(code4, tree4)]
        [InlineData(code5, tree5)]
        [InlineData(code6, tree6)]
        [InlineData(code7, tree7)]
        [InlineData(code8, tree8)]
        [InlineData(code9, tree9)]
        [InlineData(code10, tree10)]
        [InlineData(code11, tree11)]
        [InlineData(code12, tree12)]
        [InlineData(code13, tree13)]
        [InlineData(code14, tree14)]
        [InlineData(code15, tree15)]
        [InlineData(code16, tree16)]
        [InlineData(code17, tree17)]
        [InlineData(code18, tree18)]
        [InlineData(code19, tree19)]
        [InlineData(code20, tree20)]
        [InlineData(code21, tree21)]
        public void TestExpectedParsing(string input, string expected)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(input, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            string treeDump = SyntaxTreeDumper.Dump(parser.SyntaxTree);

            // System.Console.WriteLine(input);
            // System.Console.WriteLine(treeDump);

            Assert.Equal(expected, treeDump);
        }

        const string errInput1 = @"Sub a";
        const string errInput2 = @"If x < 1 Then
Sub b
EndSub
EndIf";
        const string errInput3 = @"Sub a
Sub b
EndSub";
        const string errInput4 = @"x = 1
EndSub";
        const string errInput5 = @"x = 1
ElseIf x < 1 Then
EndIf";
        const string errInput6 = @"x = 1
Else
EndIf";
        const string errInput7 = @"x = 1
EndIf";
        const string errInput8 = @"x = 1
If x > 1 Then
Else
ElseIf x < 1
EndIf";
        const string errInput9 = @"x = 1
EndWhile";
        const string errInput10 = @"x = 1
Then";
        const string errInput11 = @"
If x = 1 Then y
EndIf";
        const string errInput12 = @"x = .";
        const string errInput13 = @"x =";
        const string errInput14 = @"While .
EndWhile";
        const string errInput15 = @"If x < 1
EndIf";
        const string errInput16 = @"If x < 1 Then
ElseIf x > 1
EndIf";
        const string errInput17 = @"If x < 1 Step
EndIf";
        const string errInput18 = @"If x > 1 Then
ElseIf x < 1 Step
EndIf";
        const string errInput19 = @"For x  = 1 To 2 Step 1 :
EndFor";
        const string errInput20 = @"TextWindow.WriteLine(5";
        const string errInput21 = @"x = a[1";
        const string errInput22 = @"x = Math.Power(1 4)";
        const string errInput23 = @"x = Math.Sin(, )";
        const string errInput24 = @"Sub abc
  Sub def
  EndSub
EndSub
abc()
";

        [Theory]
        [InlineData(errInput1, new DiagnosticCode[] {DiagnosticCode.UnexpectedEndOfStream})]
        [InlineData(errInput2, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement,
            DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput3, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput4, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput5, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement,
            DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput6, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement,
            DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput7, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput8, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound,
            DiagnosticCode.UnexpectedTokenInsteadOfStatement,
            DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput9, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput10, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(errInput11, new DiagnosticCode[] {DiagnosticCode.UnexpectedStatementInsteadOfNewLine})]
        [InlineData(errInput12, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput13, new DiagnosticCode[] {DiagnosticCode.UnexpectedEndOfStream})]
        [InlineData(errInput14, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput15, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput16, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput17, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound,
            DiagnosticCode.UnexpectedStatementInsteadOfNewLine})]
        [InlineData(errInput18, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound,
            DiagnosticCode.UnexpectedStatementInsteadOfNewLine})]
        [InlineData(errInput19, new DiagnosticCode[] {DiagnosticCode.UnexpectedStatementInsteadOfNewLine})]
        [InlineData(errInput20, new DiagnosticCode[] {DiagnosticCode.UnexpectedEndOfStream})]
        [InlineData(errInput21, new DiagnosticCode[] {DiagnosticCode.UnexpectedEndOfStream})]
        [InlineData(errInput22, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput23, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound})]
        [InlineData(errInput24, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenFound,
            DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        public void TestErrorCases(string errInput, DiagnosticCode[] errDiagnostics)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(errInput, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(tokens, diagnostics);
            Assert.Equal(errDiagnostics.Length, diagnostics.Contents.Count);
            for (int i = 0; i < errDiagnostics.Length; i++)
            {
                Assert.Equal(errDiagnostics[i], diagnostics.Contents[i].Code);
            }
        }

        const string unsupportedInput1 = @".5";
        const string unsupportedInput2 = @"a = 1 : b = 2";
        const string unsupportedInput3 = @"If a = 1 Then b = 2 EndIf";
        [Theory]
        [InlineData(unsupportedInput1, new DiagnosticCode[] {DiagnosticCode.UnexpectedTokenInsteadOfStatement})]
        [InlineData(unsupportedInput2, new DiagnosticCode[] {DiagnosticCode.UnexpectedStatementInsteadOfNewLine})]
        [InlineData(unsupportedInput3, new DiagnosticCode[] {DiagnosticCode.UnexpectedStatementInsteadOfNewLine,
            DiagnosticCode.UnexpectedEndOfStream})]
        public void TestUnsupportedParsing(string errInput, DiagnosticCode[] errDiagnostics)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            var tokens = Scanner.Scan(errInput, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(tokens, diagnostics);
            Assert.Equal(errDiagnostics.Length, diagnostics.Contents.Count);
            for (int i = 0; i < errDiagnostics.Length; i++)
            {
                Assert.Equal(errDiagnostics[i], diagnostics.Contents[i].Code);
            }
        }
    }
}