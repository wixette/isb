// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using ISB.Properties;

namespace ISB.Utilities
{
    public enum DiagnosticCode
    {
        AssigningNonSubModuleToEvent,
        ExpectedALeftValue,
        ExpectedExpressionWithAValue,
        GoToUndefinedLabel,
        InvalidExpressionStatement,
        LibraryMemberDeprecatedFromOlderVersion,
        LibraryMemberNeedsDesktop,
        LibraryMemberNotFound,
        PropertyHasNoSetter,
        TwoLabelsWithTheSameName,
        TwoSubModulesWithTheSameName,
        UnassignedExpressionStatement,
        UnexpectedArgumentsCount,
        UnexpectedEndOfStream,
        UnexpectedStatementInsteadOfNewLine,
        UnexpectedTokenFound,
        UnexpectedTokenInsteadOfStatement,
        UnrecognizedCharacter,
        UnsupportedArrayBaseExpression,
        UnsupportedDotBaseExpression,
        UnsupportedInvocationBaseExpression,
        UnterminatedStringLiteral,
        ValueIsNotANumber,
    }

    internal static partial class DiagnosticCodeExtensions
    {
        public static string ToDisplayString(this DiagnosticCode kind)
        {
            switch (kind)
            {
                case DiagnosticCode.AssigningNonSubModuleToEvent: return Resources.AssigningNonSubModuleToEvent;
                case DiagnosticCode.ExpectedALeftValue: return Resources.ExpectedALeftValue;
                case DiagnosticCode.ExpectedExpressionWithAValue: return Resources.ExpectedExpressionWithAValue;
                case DiagnosticCode.GoToUndefinedLabel: return Resources.GoToUndefinedLabel;
                case DiagnosticCode.InvalidExpressionStatement: return Resources.InvalidExpressionStatement;
                case DiagnosticCode.LibraryMemberDeprecatedFromOlderVersion: return Resources.LibraryMemberDeprecatedFromOlderVersion;
                case DiagnosticCode.LibraryMemberNeedsDesktop: return Resources.LibraryMemberNeedsDesktop;
                case DiagnosticCode.LibraryMemberNotFound: return Resources.LibraryMemberNotFound;
                case DiagnosticCode.PropertyHasNoSetter: return Resources.PropertyHasNoSetter;
                case DiagnosticCode.TwoLabelsWithTheSameName: return Resources.TwoLabelsWithTheSameName;
                case DiagnosticCode.TwoSubModulesWithTheSameName: return Resources.TwoSubModulesWithTheSameName;
                case DiagnosticCode.UnassignedExpressionStatement: return Resources.UnassignedExpressionStatement;
                case DiagnosticCode.UnexpectedArgumentsCount: return Resources.UnexpectedArgumentsCount;
                case DiagnosticCode.UnexpectedEndOfStream: return Resources.UnexpectedEndOfStream;
                case DiagnosticCode.UnexpectedStatementInsteadOfNewLine: return Resources.UnexpectedStatementInsteadOfNewLine;
                case DiagnosticCode.UnexpectedTokenFound: return Resources.UnexpectedTokenFound;
                case DiagnosticCode.UnexpectedTokenInsteadOfStatement: return Resources.UnexpectedTokenInsteadOfStatement;
                case DiagnosticCode.UnrecognizedCharacter: return Resources.UnrecognizedCharacter;
                case DiagnosticCode.UnsupportedArrayBaseExpression: return Resources.UnsupportedArrayBaseExpression;
                case DiagnosticCode.UnsupportedDotBaseExpression: return Resources.UnsupportedDotBaseExpression;
                case DiagnosticCode.UnsupportedInvocationBaseExpression: return Resources.UnsupportedInvocationBaseExpression;
                case DiagnosticCode.UnterminatedStringLiteral: return Resources.UnterminatedStringLiteral;
                case DiagnosticCode.ValueIsNotANumber: return Resources.ValueIsNotANumber;
                default: throw ExceptionUtilities.UnexpectedEnumValue(kind);
            }
        }
    }
}