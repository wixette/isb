// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using ISB.Utilities;

namespace ISB.Parsing
{
    internal enum SyntaxNodeKind
    {
        SubModuleStatementSyntax,
        StatementBlockSyntax,
        IfPartSyntax,
        ElseIfPartSyntax,
        ElseIfPartGroupSyntax,
        ElsePartSyntax,
        IfStatementSyntax,
        WhileStatementSyntax,
        ForStepClauseSyntax,
        ForStatementSyntax,
        LabelStatementSyntax,
        GoToStatementSyntax,
        UnrecognizedStatementSyntax,
        ExpressionStatementSyntax,
        CommentStatementSyntax,
        UnaryOperatorExpressionSyntax,
        BinaryOperatorExpressionSyntax,
        ObjectAccessExpressionSyntax,
        ArrayAccessExpressionSyntax,
        ArgumentSyntax,
        ArgumentGroupSyntax,
        InvocationExpressionSyntax,
        ParenthesisExpressionSyntax,
        IdentifierExpressionSyntax,
        StringLiteralExpressionSyntax,
        NumberLiteralExpressionSyntax,
        UnrecognizedExpressionSyntax,
        TerminatorSyntax
    }

    internal static partial class SyntaxNodeKindExtensions
    {
        public static string ToDisplayString(this SyntaxNodeKind kind)
        {
            switch (kind)
            {
                case SyntaxNodeKind.SubModuleStatementSyntax: return "SubModuleStatementSyntax";
                case SyntaxNodeKind.StatementBlockSyntax: return "StatementBlockSyntax";
                case SyntaxNodeKind.IfPartSyntax: return "IfPartSyntax";
                case SyntaxNodeKind.ElseIfPartSyntax: return "ElseIfPartSyntax";
                case SyntaxNodeKind.ElsePartSyntax: return "ElsePartSyntax";
                case SyntaxNodeKind.ElseIfPartGroupSyntax: return "ElseIfPartGroupSyntax";
                case SyntaxNodeKind.IfStatementSyntax: return "IfStatementSyntax";
                case SyntaxNodeKind.WhileStatementSyntax: return "WhileStatementSyntax";
                case SyntaxNodeKind.ForStepClauseSyntax: return "ForStepClauseSyntax";
                case SyntaxNodeKind.ForStatementSyntax: return "ForStatementSyntax";
                case SyntaxNodeKind.LabelStatementSyntax: return "LabelStatementSyntax";
                case SyntaxNodeKind.GoToStatementSyntax: return "GoToStatementSyntax";
                case SyntaxNodeKind.UnrecognizedStatementSyntax: return "UnrecognizedStatementSyntax";
                case SyntaxNodeKind.ExpressionStatementSyntax: return "ExpressionStatementSyntax";
                case SyntaxNodeKind.CommentStatementSyntax: return "CommentStatementSyntax";
                case SyntaxNodeKind.UnaryOperatorExpressionSyntax: return "UnaryOperatorExpressionSyntax";
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax: return "BinaryOperatorExpressionSyntax";
                case SyntaxNodeKind.ObjectAccessExpressionSyntax: return "ObjectAccessExpressionSyntax";
                case SyntaxNodeKind.ArrayAccessExpressionSyntax: return "ArrayAccessExpressionSyntax";
                case SyntaxNodeKind.ArgumentSyntax: return "ArgumentSyntax";
                case SyntaxNodeKind.ArgumentGroupSyntax: return "ArgumentGroupSyntax";
                case SyntaxNodeKind.InvocationExpressionSyntax: return "InvocationExpressionSyntax";
                case SyntaxNodeKind.ParenthesisExpressionSyntax: return "ParenthesisExpressionSyntax";
                case SyntaxNodeKind.IdentifierExpressionSyntax: return "IdentifierExpressionSyntax";
                case SyntaxNodeKind.StringLiteralExpressionSyntax: return "StringLiteralExpressionSyntax";
                case SyntaxNodeKind.NumberLiteralExpressionSyntax: return "NumberLiteralExpressionSyntax";
                case SyntaxNodeKind.UnrecognizedExpressionSyntax: return "UnrecognizedExpressionSyntax";
                case SyntaxNodeKind.TerminatorSyntax: return "TerminatorSyntax";
                default: throw ExceptionUtilities.UnexpectedValue(kind);
            }
        }
    }
}
