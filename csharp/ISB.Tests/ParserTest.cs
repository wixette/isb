using System;
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
  IdentifierExpressionSyntax: a
";

        const string code2 = @"3.14";
        const string tree2 = @"StatementBlockSyntax
  NumberLiteralExpressionSyntax: 3.14
";

        const string code3 = @"""Hello""";
        const string tree3 = @"StatementBlockSyntax
  StringLiteralExpressionSyntax: ""Hello""
";

        const string code4 = @"3.14 * r * r";
        const string tree4 = @"StatementBlockSyntax
  BinaryOperatorExpressionSyntax
    BinaryOperatorExpressionSyntax
      NumberLiteralExpressionSyntax: 3.14
      PunctuationSyntax: *
      IdentifierExpressionSyntax: r
    PunctuationSyntax: *
    IdentifierExpressionSyntax: r
";

        const string code5 = @"a = ""Hello""";
        const string tree5 = @"StatementBlockSyntax
  BinaryOperatorExpressionSyntax
    IdentifierExpressionSyntax: a
    PunctuationSyntax: =
    StringLiteralExpressionSyntax: ""Hello""
";

        const string code6 = @"a = (6 + -3 * 2 - (7 - (x + (y))))";
        const string tree6 = @"StatementBlockSyntax
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
  InvocationExpressionSyntax
    IdentifierExpressionSyntax: Cos
    PunctuationSyntax: (
    EmptySyntax
    PunctuationSyntax: )
";

        const string code8 = @"Math.Pow(x, y + 0.5)";
        const string tree8 = @"StatementBlockSyntax
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
          BinaryOperatorExpressionSyntax
            IdentifierExpressionSyntax: b
            PunctuationSyntax: =
            NumberLiteralExpressionSyntax: 5
    ElsePartSyntax
      KeywordSyntax: Else
      StatementBlockSyntax
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
        public void TestExpectedParsing(string input, string expected)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(input, diagnostics);
            Assert.Empty(diagnostics.Contents);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            Assert.Empty(diagnostics.Contents);
            string treeDump = SyntaxTreeDumper.Dump(parser.SyntaxTree);
            Console.WriteLine("] {0}\n{1}", input, treeDump);
            Assert.Equal(expected, treeDump);
        }

        [Fact]
        public void TestUnsupportedParsing()
        {
            // .5
            // a = 1 : b = 2
        }

        [Fact]
        public void TestErrorCases()
        {
        }
    }
}