// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Utilities;

namespace ISB.Runtime
{
    public sealed class Engine
    {
        private Environment env = new Environment();
        private DiagnosticBag diagnostics = new DiagnosticBag();
        private Compiler compiler;

        public Engine()
        {
            compiler = new Compiler(this.env, "Program", this.diagnostics);
            this.Reset();
        }

        public void Reset()
        {
            this.env.Reset();
            this.diagnostics.Reset();
            this.compiler.Reset();
        }

        public bool HasError => this.diagnostics.Contents.Count > 0;

        public DiagnosticBag ErrorInfo => this.diagnostics;

        public string AssemblyInTextFormat => compiler.Instructions.ToTextFormat();

        public bool Compile(string code, bool isIncremental)
        {
            if (!isIncremental)
            {
                this.Reset();
            }
            else
            {
                // Incremental mode is typically used by an interactive shell.
                // Diagnostics must be cleared every time when a new code piece is being compiled.
                this.diagnostics.Reset();
            }
            var tokens = Scanner.Scan(code, diagnostics);
            var tree = Parser.Parse(tokens, diagnostics);
            if (this.HasError)
            {
                // So far we don't mix the stage of scanning/parsing and the stage of IR generating.
                return false;
            }
            this.compiler.Compile(tree);
            return !this.HasError;
        }
    }
}