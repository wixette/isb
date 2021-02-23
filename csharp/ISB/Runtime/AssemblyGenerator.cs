// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;

using ISB.Parsing;
using ISB.Utilities;

namespace ISB.Runtime
{
    public sealed class AssemblyGenerator
    {
        private readonly DiagnosticBag diagnostics;
        private readonly string moduleName;
        private int instructionIndex;

        public Assembly AssemblyBlock { get; private set; }

        public AssemblyGenerator(string moduleName,
            SyntaxNode treeRoot,
            IDictionary<string, int> labelDictionary,
            int startInstructionNo = 0)
        {
            this.moduleName = moduleName;
            this.instructionIndex = Math.Max(0, startInstructionNo);
            this.AssemblyBlock = new Assembly();
        }
    }
}