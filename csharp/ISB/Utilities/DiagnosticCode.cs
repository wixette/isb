// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using ISB.Properties;

namespace ISB.Utilities
{
    public enum DiagnosticCode
    {
        UnrecognizedCharacter,
        UnterminatedStringLiteral,
        UnexpectedTokenFound,
        UnexpectedEndOfStream,
        UnexpectedStatementInsteadOfNewLine,
        UnexpectedTokenInsteadOfStatement,
        TwoSubModulesWithTheSameName,
        TwoLabelsWithTheSameName,
        GoToUndefinedLabel,
        PropertyHasNoSetter,
        AssigningNonSubModuleToEvent,
        UnassignedExpressionStatement,
        InvalidExpressionStatement,
        UnsupportedArrayBaseExpression,
        ValueIsNotANumber,
        UnsupportedDotBaseExpression,
        ExpectedExpressionWithAValue,
        LibraryMemberNotFound,
        UnexpectedArgumentsCount,
        UnsupportedInvocationBaseExpression,
        LibraryMemberDeprecatedFromOlderVersion,
        LibraryMemberNeedsDesktop,
    }

    internal static partial class DiagnosticCodeExtensions
    {
        public static string ToDisplayString(this DiagnosticCode kind)
        {
            switch (kind)
            {
                case DiagnosticCode.UnrecognizedCharacter: return Resources.UnrecognizedCharacter;
                case DiagnosticCode.UnterminatedStringLiteral: return Resources.UnterminatedStringLiteral;
                case DiagnosticCode.UnexpectedTokenFound: return Resources.UnexpectedTokenFound;
                case DiagnosticCode.UnexpectedEndOfStream: return Resources.UnexpectedEndOfStream;
                case DiagnosticCode.UnexpectedStatementInsteadOfNewLine: return Resources.UnexpectedStatementInsteadOfNewLine;
                case DiagnosticCode.UnexpectedTokenInsteadOfStatement: return Resources.UnexpectedTokenInsteadOfStatement;
                case DiagnosticCode.TwoSubModulesWithTheSameName: return Resources.TwoSubModulesWithTheSameName;
                case DiagnosticCode.TwoLabelsWithTheSameName: return Resources.TwoLabelsWithTheSameName;
                case DiagnosticCode.GoToUndefinedLabel: return Resources.GoToUndefinedLabel;
                case DiagnosticCode.PropertyHasNoSetter: return Resources.PropertyHasNoSetter;
                case DiagnosticCode.AssigningNonSubModuleToEvent: return Resources.AssigningNonSubModuleToEvent;
                case DiagnosticCode.UnassignedExpressionStatement: return Resources.UnassignedExpressionStatement;
                case DiagnosticCode.InvalidExpressionStatement: return Resources.InvalidExpressionStatement;
                case DiagnosticCode.UnsupportedArrayBaseExpression: return Resources.UnsupportedArrayBaseExpression;
                case DiagnosticCode.ValueIsNotANumber: return Resources.ValueIsNotANumber;
                case DiagnosticCode.UnsupportedDotBaseExpression: return Resources.UnsupportedDotBaseExpression;
                case DiagnosticCode.ExpectedExpressionWithAValue: return Resources.ExpectedExpressionWithAValue;
                case DiagnosticCode.LibraryMemberNotFound: return Resources.LibraryMemberNotFound;
                case DiagnosticCode.UnexpectedArgumentsCount: return Resources.UnexpectedArgumentsCount;
                case DiagnosticCode.UnsupportedInvocationBaseExpression: return Resources.UnsupportedInvocationBaseExpression;
                case DiagnosticCode.LibraryMemberDeprecatedFromOlderVersion: return Resources.LibraryMemberDeprecatedFromOlderVersion;
                case DiagnosticCode.LibraryMemberNeedsDesktop: return Resources.LibraryMemberNeedsDesktop;
                default: throw ExceptionUtilities.UnexpectedValue(kind);
            }
        }
    }
}