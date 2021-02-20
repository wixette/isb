// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("ISB.Tests")]

namespace ISB.Scanner
{
    [DebuggerDisplay("{ToDisplayString()}")]
    internal sealed class Token
    {
        public Token(TokenKind kind, string text, TextRange range)
        {
            Debug.Assert(range.Start.Line == range.End.Line,
                "Tokens should never span multiple lines");

            this.Kind = kind;
            this.Text = text;
            this.Range = range;
        }

        public TokenKind Kind { get; private set; }

        public string Text { get; private set; }

        public TextRange Range { get; private set; }

        public string ToDisplayString() =>
            $"{nameof(TokenKind)}.{this.Kind}: '{this.Text}' at {this.Range.ToDisplayString()}";
    }
}
