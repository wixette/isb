// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ISB.Properties;
using ISB.Scanning;

namespace ISB.Utilities
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed class Diagnostic
    {
        // How to Add/remove error code.
        //
        // (1) Add or remove ErrorCode entries in the following enum. Please make sure the entries are listed in
        //    the alphabetic order.
        // (2) Add or remove messages in Properties/Resources.resx.
        // (3) Open the project in Visual Studio (since VS Code doesn't support auto Designer class generation) and
        //    use the IDE to auto update Properties/Resources.Designer.cs.
        // (4) Add or remove "public static Diagnostic Report..." method in this class. Please make sure the Report...
        //    methods are listed in the alphabetic order.
        public enum ErrorCode
        {
            AssigningNonSubModuleToEvent,
            ExpectedALeftValue,
            ExpectedExpressionWithAValue,
            GoToUndefinedLabel,
            InvalidExpressionStatement,
            LibraryMemberNotFound,
            PropertyHasNoSetter,
            RuntimeError,
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

        public static Diagnostic ReportAssigningNonSubModuleToEvent(TextRange range)
        {
            return new Diagnostic(ErrorCode.AssigningNonSubModuleToEvent, range,
                Resources.AssigningNonSubModuleToEvent);
        }

        public static Diagnostic ReportExpectedALeftValue(TextRange range)
        {
            return new Diagnostic(ErrorCode.ExpectedALeftValue, range,
                Resources.ExpectedALeftValue);
        }

        public static Diagnostic ReportExpectedExpressionWithAValue(TextRange range)
        {
            return new Diagnostic(ErrorCode.ExpectedExpressionWithAValue, range,
                Resources.ExpectedExpressionWithAValue);
        }

        public static Diagnostic ReportGoToUndefinedLabel(TextRange range, string label)
        {
            return new Diagnostic(ErrorCode.GoToUndefinedLabel, range,
                Resources.GoToUndefinedLabel, label);
        }

        public static Diagnostic ReportInvalidExpressionStatement(TextRange range)
        {
            return new Diagnostic(ErrorCode.InvalidExpressionStatement, range,
                Resources.InvalidExpressionStatement);
        }

        public static Diagnostic ReportLibraryMemberNotFound(TextRange range, string library, string member)
        {
            return new Diagnostic(ErrorCode.LibraryMemberNotFound, range,
                Resources.LibraryMemberNotFound, library, member);
        }

        public static Diagnostic ReportPropertyHasNoSetter(TextRange range, string library, string property)
        {
            return new Diagnostic(ErrorCode.PropertyHasNoSetter, range,
                Resources.PropertyHasNoSetter, library, property);
        }

        public static Diagnostic ReportRuntimeError(TextRange range, string description)
        {
            return new Diagnostic(ErrorCode.RuntimeError, range,
                Resources.RuntimeError, description);
        }

        public static Diagnostic ReportTwoLabelsWithTheSameName(TextRange range, string label)
        {
            return new Diagnostic(ErrorCode.TwoLabelsWithTheSameName, range,
                Resources.TwoLabelsWithTheSameName, label);
        }

        public static Diagnostic ReportTwoSubModulesWithTheSameName(TextRange range, string name)
        {
            return new Diagnostic(ErrorCode.TwoSubModulesWithTheSameName, range,
                Resources.TwoSubModulesWithTheSameName, name);
        }

        public static Diagnostic ReportUnassignedExpressionStatement(TextRange range)
        {
            return new Diagnostic(ErrorCode.UnassignedExpressionStatement, range,
                Resources.UnassignedExpressionStatement);
        }

        public static Diagnostic ReportUnexpectedArgumentsCount(TextRange range, int actualCount, int expectedCount)
        {
            return new Diagnostic(ErrorCode.UnexpectedArgumentsCount, range,
                Resources.UnexpectedArgumentsCount, actualCount.ToString(), expectedCount.ToString());
        }

        public static Diagnostic ReportUnexpectedEndOfStream(TextRange range, TokenKind expected)
        {
            return new Diagnostic(ErrorCode.UnexpectedEndOfStream, range,
                Resources.UnexpectedEndOfStream, expected.ToDisplayString());
        }

        public static Diagnostic ReportUnexpectedStatementInsteadOfNewLine(TextRange range)
        {
            return new Diagnostic(ErrorCode.UnexpectedStatementInsteadOfNewLine, range,
                Resources.UnexpectedStatementInsteadOfNewLine);
        }

        public static Diagnostic ReportUnexpectedTokenFound(TextRange range, TokenKind found, TokenKind expected)
        {
            return new Diagnostic(ErrorCode.UnexpectedTokenFound, range,
                Resources.UnexpectedTokenFound, found.ToDisplayString(), expected.ToDisplayString());
        }

        public static Diagnostic ReportUnexpectedTokenInsteadOfStatement(TextRange range, TokenKind found)
        {
            return new Diagnostic(ErrorCode.UnexpectedTokenInsteadOfStatement, range,
                Resources.UnexpectedTokenInsteadOfStatement, found.ToDisplayString());
        }

        public static Diagnostic ReportUnrecognizedCharacter(TextRange range, char character)
        {
            return new Diagnostic(ErrorCode.UnrecognizedCharacter, range,
                Resources.UnrecognizedCharacter, character.ToString());
        }

        public static Diagnostic ReportUnsupportedArrayBaseExpression(TextRange range)
        {
            return new Diagnostic(ErrorCode.UnsupportedArrayBaseExpression, range,
                Resources.UnsupportedArrayBaseExpression);
        }

        public static Diagnostic ReportUnsupportedDotBaseExpression(TextRange range)
        {
            return new Diagnostic(ErrorCode.UnsupportedDotBaseExpression, range,
                Resources.UnsupportedDotBaseExpression);
        }

        public static Diagnostic ReportUnsupportedInvocationBaseExpression(TextRange range)
        {
            return new Diagnostic(ErrorCode.UnsupportedInvocationBaseExpression, range,
                Resources.UnsupportedInvocationBaseExpression);
        }

        public static Diagnostic ReportUnterminatedStringLiteral(TextRange range)
        {
            return new Diagnostic(ErrorCode.UnterminatedStringLiteral, range,
                Resources.UnterminatedStringLiteral);
        }

        public static Diagnostic ReportValueIsNotANumber(TextRange range, string value)
        {
            return new Diagnostic(ErrorCode.ValueIsNotANumber, range,
                Resources.ValueIsNotANumber, value);
        }

        private string message;
        private string[] args;

        private Diagnostic(ErrorCode code, TextRange range, string message, params string[] args)
        {
            this.Code = code;
            this.Range = range;
            this.message = message;
            this.args = args;
        }

        public ErrorCode Code { get; private set; }

        public TextRange Range { get; private set; }

        public IReadOnlyList<string> Args => this.args;

        public string ToDisplayString() =>
            string.Format(CultureInfo.CurrentCulture, message, this.args);
    }
}
