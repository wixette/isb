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

        public void Reset()
        {
            this.builder.Clear();
        }

        public void Add(Diagnostic diagnostic)
        {
            this.builder.Add(diagnostic);
        }
    }
}