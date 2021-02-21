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

        const string code6 = @"a = (6 + -3 * (7 - (x + (y))))";
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
          UnaryOperatorExpressionSyntax
            PunctuationSyntax: -
            NumberLiteralExpressionSyntax: 3
          PunctuationSyntax: *
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

        const string code7 = @"Cos(1)";
        const string tree7 = @"StatementBlockSyntax
  InvocationExpressionSyntax
    IdentifierExpressionSyntax: Cos
    PunctuationSyntax: (
    ArgumentGroupSyntax
      ArgumentSyntax
        NumberLiteralExpressionSyntax: 1
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
        }

        [Fact]
        public void TestErrorCases()
        {
        }
    }
}