using Xunit;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Utilities;

namespace ISB.Tests
{
    public class SyntaxNodeTest
    {
        private SyntaxNode standaloneNode;
        private SyntaxNode binaryExpressionNode;

        public SyntaxNodeTest()
        {
            standaloneNode = new SyntaxNode(SyntaxNodeKind.IdentifierExpressionSyntax,
                new Token(TokenKind.Identifier, "abc", ((0, 0), (0, 2))));

            binaryExpressionNode = new SyntaxNode(SyntaxNodeKind.BinaryOperatorExpressionSyntax,
                new SyntaxNode(SyntaxNodeKind.IdentifierExpressionSyntax,
                    new Token(TokenKind.Identifier, "abc", ((0, 0), (0, 2)))),
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax,
                    new Token(TokenKind.Equal, "=", ((0, 4), (0, 4)))),
                new SyntaxNode(SyntaxNodeKind.NumberLiteralExpressionSyntax,
                    new Token(TokenKind.NumberLiteral, "3.14", ((0, 6), (0, 9)))));
        }

        [Fact]
        public void TerminatorNode()
        {
            Assert.True(standaloneNode.IsTerminator);
            Assert.Empty(standaloneNode.Children);
            Assert.Null(standaloneNode.Parent);
            Assert.Equal(TokenKind.Identifier, standaloneNode.Terminator.Kind);
            Assert.Equal("abc", standaloneNode.Terminator.Text);
            Assert.Equal(((0, 0), (0, 2)), standaloneNode.Range);
        }

        [Fact]
        public void BinaryExpressionNode()
        {
            Assert.False(binaryExpressionNode.IsTerminator);
            Assert.Equal(3, binaryExpressionNode.Children.Count);
            Assert.Null(binaryExpressionNode.Parent);
            Assert.Equal(((0, 0), (0, 9)), binaryExpressionNode.Range);
            Assert.True(binaryExpressionNode.Children[0].IsTerminator);
            Assert.True(binaryExpressionNode.Children[1].IsTerminator);
            Assert.True(binaryExpressionNode.Children[2].IsTerminator);
            Assert.Equal(SyntaxNodeKind.IdentifierExpressionSyntax, binaryExpressionNode.Children[0].Kind);
            Assert.Equal(SyntaxNodeKind.TerminatorSyntax, binaryExpressionNode.Children[1].Kind);
            Assert.Equal(SyntaxNodeKind.NumberLiteralExpressionSyntax, binaryExpressionNode.Children[2].Kind);
            Assert.Equal(binaryExpressionNode, binaryExpressionNode.Children[0].Parent);
            Assert.Equal(binaryExpressionNode, binaryExpressionNode.Children[1].Parent);
            Assert.Equal(binaryExpressionNode, binaryExpressionNode.Children[2].Parent);
        }
    }
}