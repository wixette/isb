// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using ISB.Scanner;

namespace ISB.Utilities
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed class Diagnostic
    {
        private string[] args;

        public Diagnostic(DiagnosticCode code, TextRange range, params string[] args)
        {
            this.Code = code;
            this.Range = range;
            this.args = args;
        }

        public DiagnosticCode Code { get; private set; }

        public TextRange Range { get; private set; }

        public IReadOnlyList<string> Args => this.args;

        public string ToDisplayString() =>
            string.Format(CultureInfo.CurrentCulture, this.Code.ToDisplayString(), this.args);
    }
}