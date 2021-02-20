using System.Collections.Generic;
using Xunit;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Tests
{
    public class ScannerTest
    {
        [Fact]
        public void Test1()
        {
            DiagnosticBag bag = new DiagnosticBag();
            Scanner scanner = new Scanner("a = 3.14", bag);

            Assert.Equal(3, scanner.Tokens.Count);
            Assert.Empty(bag.Contents);

            Token t = scanner.Tokens[0];
            Assert.Equal(TokenKind.Identifier, t.Kind);
            Assert.Equal("a", t.Text);
            Assert.Equal(((0, 0), (0, 0)), t.Range);

            t = scanner.Tokens[1];
            Assert.Equal(TokenKind.Equal, t.Kind);
            Assert.Equal("=", t.Text);
            Assert.Equal(((0, 2), (0, 2)), t.Range);

            t = scanner.Tokens[2];
            Assert.Equal(TokenKind.NumberLiteral, t.Kind);
            Assert.Equal("3.14", t.Text);
            Assert.Equal(((0, 4), (0, 7)), t.Range);
        }

        [Fact]
        public void Test2()
        {
            DiagnosticBag bag = new DiagnosticBag();
            Scanner scanner = new Scanner("For I = 1 To 10 : EndFor", bag);

            Assert.Equal(8, scanner.Tokens.Count);
            Assert.Empty(bag.Contents);

            Token t = scanner.Tokens[0];
            Assert.Equal(TokenKind.For, t.Kind);
            Assert.Equal("For", t.Text);
            Assert.Equal(((0, 0), (0, 2)), t.Range);

            t = scanner.Tokens[7];
            Assert.Equal(TokenKind.EndFor, t.Kind);
            Assert.Equal("EndFor", t.Text);
            Assert.Equal(((0, 18), (0, 23)), t.Range);
        }

        [Fact]
        public void Test3()
        {
            DiagnosticBag bag = new DiagnosticBag();
            Scanner scanner = new Scanner("(a.b*(c.d--3))", bag);

            Assert.Equal(14, scanner.Tokens.Count);
            Assert.Empty(bag.Contents);

            Token t = scanner.Tokens[0];
            Assert.Equal(TokenKind.LeftParen, t.Kind);
            Assert.Equal("(", t.Text);
            Assert.Equal(((0, 0), (0, 0)), t.Range);

            t = scanner.Tokens[10];
            Assert.Equal(TokenKind.Minus, t.Kind);
            Assert.Equal("-", t.Text);
            Assert.Equal(((0, 10), (0, 10)), t.Range);
        }

        [Fact]
        public void Test4()
        {
            DiagnosticBag bag = new DiagnosticBag();
            Scanner scanner = new Scanner("x=\"abc", bag);

            Assert.Equal(3, scanner.Tokens.Count);
            Assert.Single(bag.Contents);
            Assert.Equal(DiagnosticCode.UnterminatedStringLiteral, bag.Contents[0].Code);
            Assert.Equal(((0, 2), (0, 5)), bag.Contents[0].Range);
        }

        [Fact]
        public void Test5()
        {
            DiagnosticBag bag = new DiagnosticBag();
            Scanner scanner = new Scanner("$", bag);

            Assert.Equal(1, scanner.Tokens.Count);
            Assert.Single(bag.Contents);
            Assert.Equal(DiagnosticCode.UnrecognizedCharacter, bag.Contents[0].Code);
            Assert.Equal(((0, 0), (0, 0)), bag.Contents[0].Range);
        }
    }
}
