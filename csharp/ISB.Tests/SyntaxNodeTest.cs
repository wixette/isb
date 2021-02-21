using Xunit;
using ISB.Scanning;
using ISB.Parsing;

namespace ISB.Tests
{
    public class SyntaxNodeTest
    {
        private SyntaxNode emptyNode;
        private SyntaxNode standaloneNode;
        private SyntaxNode binaryExpressionNode;

        public SyntaxNodeTest()
        {
            emptyNode = SyntaxNode.CreateEmpty();
            standaloneNode = SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax,
                new Token(TokenKind.Identifier, "abc", ((0, 0), (0, 2))));

            binaryExpressionNode = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.BinaryOperatorExpressionSyntax,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax,
                    new Token(TokenKind.Identifier, "abc", ((0, 0), (0, 2)))),
                SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax,
                    new Token(TokenKind.Equal, "=", ((0, 4), (0, 4)))),
                SyntaxNode.CreateTerminal(SyntaxNodeKind.NumberLiteralExpressionSyntax,
                    new Token(TokenKind.NumberLiteral, "3.14", ((0, 6), (0, 9)))));
        }

        [Fact]
        public void TestEmptyNode()
        {
            Assert.True(emptyNode.IsEmpty);
            Assert.True(emptyNode.IsTerminator);
            Assert.Null(emptyNode.Children);
            Assert.Null(emptyNode.Parent);
            Assert.Null(emptyNode.Terminator);
        }

        [Fact]
        public void TestTerminalNode()
        {
            Assert.True(standaloneNode.IsTerminator);
            Assert.False(standaloneNode.IsEmpty);
            Assert.Null(standaloneNode.Children);
            Assert.Null(standaloneNode.Parent);
            Assert.Equal(TokenKind.Identifier, standaloneNode.Terminator.Kind);
            Assert.Equal("abc", standaloneNode.Terminator.Text);
            Assert.Equal(((0, 0), (0, 2)), standaloneNode.Range);
        }

        [Fact]
        public void TestBinaryExpressionNode()
        {
            Assert.False(binaryExpressionNode.IsTerminator);
            Assert.False(binaryExpressionNode.IsEmpty);
            Assert.Equal(3, binaryExpressionNode.Children.Count);
            Assert.Null(binaryExpressionNode.Parent);
            Assert.Equal(((0, 0), (0, 9)), binaryExpressionNode.Range);
            Assert.True(binaryExpressionNode.Children[0].IsTerminator);
            Assert.True(binaryExpressionNode.Children[1].IsTerminator);
            Assert.True(binaryExpressionNode.Children[2].IsTerminator);
            Assert.Equal(SyntaxNodeKind.IdentifierExpressionSyntax, binaryExpressionNode.Children[0].Kind);
            Assert.Equal(SyntaxNodeKind.PunctuationSyntax, binaryExpressionNode.Children[1].Kind);
            Assert.Equal(SyntaxNodeKind.NumberLiteralExpressionSyntax, binaryExpressionNode.Children[2].Kind);
            Assert.Equal(binaryExpressionNode, binaryExpressionNode.Children[0].Parent);
            Assert.Equal(binaryExpressionNode, binaryExpressionNode.Children[1].Parent);
            Assert.Equal(binaryExpressionNode, binaryExpressionNode.Children[2].Parent);
        }

        [Fact]
        public void TestFindNodeAt()
        {
            var node = binaryExpressionNode.FindNodeAt((0, 4));
            Assert.Equal(SyntaxNodeKind.PunctuationSyntax, node.Kind);
            Assert.Equal("=", node.Terminator.Text);
            node = binaryExpressionNode.FindNodeAt((0, 9));
            Assert.Equal(SyntaxNodeKind.NumberLiteralExpressionSyntax, node.Kind);
            Assert.Equal("3.14", node.Terminator.Text);
        }
    }
}