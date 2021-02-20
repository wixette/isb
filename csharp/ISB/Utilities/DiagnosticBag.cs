// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using ISB.Scanner;

namespace ISB.Utilities
{
    internal sealed class DiagnosticBag
    {
        public readonly List<Diagnostic> Contents = new List<Diagnostic>();

        public void ReportUnrecognizedCharacter(TextRange range, char character)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnrecognizedCharacter, range, character.ToString()));
        }

        public void ReportUnterminatedStringLiteral(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnterminatedStringLiteral, range));
        }

        public void ReportUnexpectedTokenFound(TextRange range, TokenKind found, TokenKind expected)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnexpectedTokenFound, range, found.ToDisplayString(), expected.ToDisplayString()
));
        }

        public void ReportUnexpectedEndOfStream(TextRange range, TokenKind expected)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnexpectedEndOfStream, range, expected.ToDisplayString()));
        }

        public void ReportUnexpectedStatementInsteadOfNewLine(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnexpectedStatementInsteadOfNewLine, range));
        }

        public void ReportUnexpectedTokenInsteadOfStatement(TextRange range, TokenKind found)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnexpectedTokenInsteadOfStatement, range, found.ToDisplayString()));
        }

        public void ReportTwoSubModulesWithTheSameName(TextRange range, string name)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.TwoSubModulesWithTheSameName, range, name));
        }

        public void ReportTwoLabelsWithTheSameName(TextRange range, string label)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.TwoLabelsWithTheSameName, range, label));
        }

        public void ReportGoToUndefinedLabel(TextRange range, string label)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.GoToUndefinedLabel, range, label));
        }

        public void ReportPropertyHasNoSetter(TextRange range, string library, string property)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.PropertyHasNoSetter, range, library, property));
        }

        public void ReportAssigningNonSubModuleToEvent(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.AssigningNonSubModuleToEvent, range));
        }

        public void ReportUnassignedExpressionStatement(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnassignedExpressionStatement, range));
        }

        public void ReportInvalidExpressionStatement(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.InvalidExpressionStatement, range));
        }

        public void ReportUnsupportedArrayBaseExpression(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnsupportedArrayBaseExpression, range));
        }

        public void ReportValueIsNotANumber(TextRange range, string value)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.ValueIsNotANumber, range, value));
        }

        public void ReportUnsupportedDotBaseExpression(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnsupportedDotBaseExpression, range));
        }

        public void ReportExpectedExpressionWithAValue(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.ExpectedExpressionWithAValue, range));
        }

        public void ReportLibraryMemberNotFound(TextRange range, string library, string member)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.LibraryMemberNotFound, range, library, member));
        }

        public void ReportUnexpectedArgumentsCount(TextRange range, int actualCount, int expectedCount)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnexpectedArgumentsCount, range, actualCount.ToString(), expectedCount.ToString()));
        }

        public void ReportUnsupportedInvocationBaseExpression(TextRange range)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.UnsupportedInvocationBaseExpression, range));
        }

        public void ReportLibraryMemberDeprecatedFromOlderVersion(TextRange range, string library, string member)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.LibraryMemberDeprecatedFromOlderVersion, range, library, member));
        }

        public void ReportLibraryMemberNeedsDesktop(TextRange range, string library, string member)
        {
            Contents.Add(new Diagnostic(DiagnosticCode.LibraryMemberNeedsDesktop, range, library, member));
        }
    }

}