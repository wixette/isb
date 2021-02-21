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
        const string tree0 = @"";

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
      TerminatorSyntax: *
      IdentifierExpressionSyntax: r
    TerminatorSyntax: *
    IdentifierExpressionSyntax: r
";

        const string code5 = @"a = ""Hello""";
        const string tree5 = @"StatementBlockSyntax
  BinaryOperatorExpressionSyntax
    IdentifierExpressionSyntax: a
    TerminatorSyntax: =
    StringLiteralExpressionSyntax: ""Hello""
";

        const string code6 = @"a = (6 + -3 * (7 - (x + (y))))";
        const string tree6 = @"StatementBlockSyntax
  BinaryOperatorExpressionSyntax
    IdentifierExpressionSyntax: a
    TerminatorSyntax: =
    ParenthesisExpressionSyntax
      TerminatorSyntax: (
      BinaryOperatorExpressionSyntax
        NumberLiteralExpressionSyntax: 6
        TerminatorSyntax: +
        BinaryOperatorExpressionSyntax
          UnaryOperatorExpressionSyntax
            TerminatorSyntax: -
            NumberLiteralExpressionSyntax: 3
          TerminatorSyntax: *
          ParenthesisExpressionSyntax
            TerminatorSyntax: (
            BinaryOperatorExpressionSyntax
              NumberLiteralExpressionSyntax: 7
              TerminatorSyntax: -
              ParenthesisExpressionSyntax
                TerminatorSyntax: (
                BinaryOperatorExpressionSyntax
                  IdentifierExpressionSyntax: x
                  TerminatorSyntax: +
                  ParenthesisExpressionSyntax
                    TerminatorSyntax: (
                    IdentifierExpressionSyntax: y
                    TerminatorSyntax: )
                TerminatorSyntax: )
            TerminatorSyntax: )
      TerminatorSyntax: )
";

        [Theory]
        [InlineData(code0, tree0)]
        [InlineData(code1, tree1)]
        [InlineData(code2, tree2)]
        [InlineData(code3, tree3)]
        [InlineData(code4, tree4)]
        [InlineData(code5, tree5)]
        [InlineData(code6, tree6)]
        public void Test1(string input, string expected)
        {
            DiagnosticBag diagnostics = new DiagnosticBag();
            Scanner scanner = new Scanner(input, diagnostics);
            Parser parser = new Parser(scanner.Tokens, diagnostics);
            string treeDump = SyntaxTreeDumper.Dump(parser.SyntaxTree);
            Console.WriteLine("] {0}\n{1}", input, treeDump);
            Assert.Equal(expected, treeDump);
        }
    }
}