// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Diagnostics;

namespace ISB.Scanning
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public readonly struct TextRange : IEquatable<TextRange>
    {
        public TextRange(TextPosition start, TextPosition end)
        {
            this.Start = start;
            this.End = end;
        }

        public TextPosition Start { get; }

        public TextPosition End { get; }

        public static implicit operator TextRange(in (TextPosition Start, TextPosition End) tuple)
        {
            return new TextRange(tuple.Start, tuple.End);
        }

        public static TextRange None => new TextRange((-1, -1), (-1, -1));

        public static bool operator ==(TextRange left, TextRange right) =>
            left.Start == right.Start && left.End == right.End;

        public static bool operator !=(TextRange left, TextRange right) =>
            !(left == right);

        public override bool Equals(object obj) =>
            obj is TextRange other && this == other;

        public override int GetHashCode() =>
            this.Start.GetHashCode() ^ this.End.GetHashCode();

        public bool Equals(TextRange other) => this == other;

        public string ToDisplayString() =>
            $"({this.Start.ToDisplayString()}, {this.End.ToDisplayString()})";

        public override string ToString() => ToDisplayString();

        public bool Contains(in TextPosition position) =>
            this.Start <= position && position <= this.End;
    }
}
