// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using ISB.Utilities;

namespace ISB.Scanning
{
    public enum TokenKind
    {
        If,
        Then,
        Else,
        ElseIf,
        EndIf,
        For,
        To,
        Step,
        EndFor,
        While,
        EndWhile,
        Sub,
        EndSub,
        GoTo,
        Or,
        And,
        Dot,
        Comma,
        RightParen,
        LeftParen,
        RightBracket,
        LeftBracket,
        Equal,
        NotEqual,
        Plus,
        Minus,
        Multiply,
        Divide,
        Mod,
        Colon,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Identifier,
        NumberLiteral,
        StringLiteral,
        BooleanLiteral,
        Comment,
        Unrecognized,
    }

    public static partial class TokenKindExtensions
    {
        public static string ToDisplayString(this TokenKind kind)
        {
            switch (kind)
            {
                case TokenKind.If: return "If";
                case TokenKind.Then: return "Then";
                case TokenKind.Else: return "Else";
                case TokenKind.ElseIf: return "ElseIf";
                case TokenKind.EndIf: return "EndIf";
                case TokenKind.For: return "For";
                case TokenKind.To: return "To";
                case TokenKind.Step: return "Step";
                case TokenKind.EndFor: return "EndFor";
                case TokenKind.While: return "While";
                case TokenKind.EndWhile: return "EndWhile";
                case TokenKind.Sub: return "Sub";
                case TokenKind.EndSub: return "EndSub";
                case TokenKind.GoTo: return "GoTo";
                case TokenKind.Or: return "Or";
                case TokenKind.And: return "And";
                case TokenKind.Dot: return ".";
                case TokenKind.Comma: return ",";
                case TokenKind.RightParen: return ")";
                case TokenKind.LeftParen: return "(";
                case TokenKind.RightBracket: return "]";
                case TokenKind.LeftBracket: return "[";
                case TokenKind.Equal: return "=";
                case TokenKind.NotEqual: return "<>";
                case TokenKind.Plus: return "+";
                case TokenKind.Minus: return "-";
                case TokenKind.Multiply: return "*";
                case TokenKind.Divide: return "/";
                case TokenKind.Mod: return "Mod";
                case TokenKind.Colon: return ":";
                case TokenKind.LessThan: return "<";
                case TokenKind.GreaterThan: return ">";
                case TokenKind.LessThanOrEqual: return "<=";
                case TokenKind.GreaterThanOrEqual: return ">=";
                case TokenKind.Identifier: return "identifier";
                case TokenKind.NumberLiteral: return "number";
                case TokenKind.StringLiteral: return "string";
                case TokenKind.BooleanLiteral: return "boolean";
                case TokenKind.Comment: return "comment";
                case TokenKind.Unrecognized: return "unrecognized";
                default: throw ExceptionUtilities.UnexpectedEnumValue(kind);
            }
        }
    }
}
