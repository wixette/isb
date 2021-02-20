using System;
using Xunit;
using ISB.Scanner;

namespace ISB.Tests
{
    public class TokenTest
    {
        [Fact]
        public void Test1()
        {
            Token token = new Token(TokenKind.Identifier, "Console", ((0, 0), (0, 6)));
            Assert.Equal("TokenKind.Identifier: 'Console' at ((0, 0), (0, 6))",
                token.ToDisplayString());
        }

        [Fact]
        public void Test2()
        {
            Assert.ThrowsAny<Exception>(() =>
                new Token(TokenKind.Comment, "a\nb", ((0, 0), (1, 0))));
        }
    }
}
