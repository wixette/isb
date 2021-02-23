// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using ISB.Utilities;

namespace ISB.Parsing
{
    public enum SyntaxNodeKind
    {
        // Non-terminal syntax node kinds.
        ArgumentGroupSyntax,
        ArgumentSyntax,
        ArrayAccessExpressionSyntax,
        BinaryOperatorExpressionSyntax,
        ElseIfPartGroupSyntax,
        ElseIfPartSyntax,
        ElsePartSyntax,
        ExpressionStatementSyntax,
        ForStatementSyntax,
        ForStepClauseSyntax,
        GoToStatementSyntax,
        IfPartSyntax,
        IfStatementSyntax,
        InvocationExpressionSyntax,
        LabelStatementSyntax,
        ObjectAccessExpressionSyntax,
        ParenthesisExpressionSyntax,
        StatementBlockSyntax,
        SubModuleStatementSyntax,
        UnaryOperatorExpressionSyntax,
        WhileStatementSyntax,
        // Terminal syntax node kinds.
        CommentStatementSyntax,
        EmptySyntax,
        IdentifierExpressionSyntax,
        KeywordSyntax,
        NumberLiteralExpressionSyntax,
        PunctuationSyntax,
        StringLiteralExpressionSyntax,
        UnrecognizedExpressionSyntax,
        UnrecognizedStatementSyntax,
    }

    public static partial class SyntaxNodeKindExtensions
    {
        public static string ToDisplayString(this SyntaxNodeKind kind)
        {
            switch (kind)
            {
                // Non-terminal syntax node kinds.
                case SyntaxNodeKind.ArgumentGroupSyntax: return "ArgumentGroupSyntax";
                case SyntaxNodeKind.ArgumentSyntax: return "ArgumentSyntax";
                case SyntaxNodeKind.ArrayAccessExpressionSyntax: return "ArrayAccessExpressionSyntax";
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax: return "BinaryOperatorExpressionSyntax";
                case SyntaxNodeKind.ElseIfPartGroupSyntax: return "ElseIfPartGroupSyntax";
                case SyntaxNodeKind.ElseIfPartSyntax: return "ElseIfPartSyntax";
                case SyntaxNodeKind.ElsePartSyntax: return "ElsePartSyntax";
                case SyntaxNodeKind.ExpressionStatementSyntax: return "ExpressionStatementSyntax";
                case SyntaxNodeKind.ForStatementSyntax: return "ForStatementSyntax";
                case SyntaxNodeKind.ForStepClauseSyntax: return "ForStepClauseSyntax";
                case SyntaxNodeKind.GoToStatementSyntax: return "GoToStatementSyntax";
                case SyntaxNodeKind.IfPartSyntax: return "IfPartSyntax";
                case SyntaxNodeKind.IfStatementSyntax: return "IfStatementSyntax";
                case SyntaxNodeKind.InvocationExpressionSyntax: return "InvocationExpressionSyntax";
                case SyntaxNodeKind.LabelStatementSyntax: return "LabelStatementSyntax";
                case SyntaxNodeKind.ObjectAccessExpressionSyntax: return "ObjectAccessExpressionSyntax";
                case SyntaxNodeKind.ParenthesisExpressionSyntax: return "ParenthesisExpressionSyntax";
                case SyntaxNodeKind.StatementBlockSyntax: return "StatementBlockSyntax";
                case SyntaxNodeKind.SubModuleStatementSyntax: return "SubModuleStatementSyntax";
                case SyntaxNodeKind.UnaryOperatorExpressionSyntax: return "UnaryOperatorExpressionSyntax";
                case SyntaxNodeKind.WhileStatementSyntax: return "WhileStatementSyntax";
                // Terminal syntax node kinds.
                case SyntaxNodeKind.CommentStatementSyntax: return "CommentStatementSyntax";
                case SyntaxNodeKind.EmptySyntax: return "EmptySyntax";
                case SyntaxNodeKind.IdentifierExpressionSyntax: return "IdentifierExpressionSyntax";
                case SyntaxNodeKind.KeywordSyntax: return "KeywordSyntax";
                case SyntaxNodeKind.NumberLiteralExpressionSyntax: return "NumberLiteralExpressionSyntax";
                case SyntaxNodeKind.PunctuationSyntax: return "PunctuationSyntax";
                case SyntaxNodeKind.StringLiteralExpressionSyntax: return "StringLiteralExpressionSyntax";
                case SyntaxNodeKind.UnrecognizedExpressionSyntax: return "UnrecognizedExpressionSyntax";
                case SyntaxNodeKind.UnrecognizedStatementSyntax: return "UnrecognizedStatementSyntax";
                default: throw ExceptionUtilities.UnexpectedValue(kind);
            }
        }
    }
}
