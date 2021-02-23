// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using ISB.Scanning;

namespace ISB.Utilities
{
    public sealed class DiagnosticBag
    {
        private readonly List<Diagnostic> builder = new List<Diagnostic>();

        public IReadOnlyList<Diagnostic> Contents => this.builder;

        public void ReportUnrecognizedCharacter(TextRange range, char character)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnrecognizedCharacter, range, character.ToString()));
        }

        public void ReportUnterminatedStringLiteral(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnterminatedStringLiteral, range));
        }

        public void ReportUnexpectedTokenFound(TextRange range, TokenKind found, TokenKind expected)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnexpectedTokenFound, range, found.ToDisplayString(), expected.ToDisplayString()
));
        }

        public void ReportUnexpectedEndOfStream(TextRange range, TokenKind expected)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnexpectedEndOfStream, range, expected.ToDisplayString()));
        }

        public void ReportUnexpectedStatementInsteadOfNewLine(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnexpectedStatementInsteadOfNewLine, range));
        }

        public void ReportUnexpectedTokenInsteadOfStatement(TextRange range, TokenKind found)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnexpectedTokenInsteadOfStatement, range, found.ToDisplayString()));
        }

        public void ReportTwoSubModulesWithTheSameName(TextRange range, string name)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.TwoSubModulesWithTheSameName, range, name));
        }

        public void ReportTwoLabelsWithTheSameName(TextRange range, string label)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.TwoLabelsWithTheSameName, range, label));
        }

        public void ReportGoToUndefinedLabel(TextRange range, string label)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.GoToUndefinedLabel, range, label));
        }

        public void ReportPropertyHasNoSetter(TextRange range, string library, string property)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.PropertyHasNoSetter, range, library, property));
        }

        public void ReportAssigningNonSubModuleToEvent(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.AssigningNonSubModuleToEvent, range));
        }

        public void ReportUnassignedExpressionStatement(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnassignedExpressionStatement, range));
        }

        public void ReportInvalidExpressionStatement(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.InvalidExpressionStatement, range));
        }

        public void ReportUnsupportedArrayBaseExpression(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnsupportedArrayBaseExpression, range));
        }

        public void ReportValueIsNotANumber(TextRange range, string value)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.ValueIsNotANumber, range, value));
        }

        public void ReportUnsupportedDotBaseExpression(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnsupportedDotBaseExpression, range));
        }

        public void ReportExpectedExpressionWithAValue(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.ExpectedExpressionWithAValue, range));
        }

        public void ReportLibraryMemberNotFound(TextRange range, string library, string member)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.LibraryMemberNotFound, range, library, member));
        }

        public void ReportUnexpectedArgumentsCount(TextRange range, int actualCount, int expectedCount)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnexpectedArgumentsCount, range, actualCount.ToString(), expectedCount.ToString()));
        }

        public void ReportUnsupportedInvocationBaseExpression(TextRange range)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.UnsupportedInvocationBaseExpression, range));
        }

        public void ReportLibraryMemberDeprecatedFromOlderVersion(TextRange range, string library, string member)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.LibraryMemberDeprecatedFromOlderVersion, range, library, member));
        }

        public void ReportLibraryMemberNeedsDesktop(TextRange range, string library, string member)
        {
            this.builder.Add(new Diagnostic(DiagnosticCode.LibraryMemberNeedsDesktop, range, library, member));
        }
    }

}