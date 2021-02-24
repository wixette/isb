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
        private readonly int sequenceBase;
        private int sequenceIndex;

        public Assembly AssemblyBlock { get; private set; }

        public AssemblyGenerator(string moduleName,
            SyntaxNode treeRoot,
            IDictionary<string, int> labelDictionary,
            int startInstructionNo = 0)
        {
            this.moduleName = moduleName;
            this.sequenceBase = Math.Max(0, startInstructionNo);
            this.sequenceIndex = this.sequenceBase;
            this.AssemblyBlock = new Assembly();

            this.Generate(treeRoot, diagnostics);
        }

        private void Generate(SyntaxNode node, DiagnosticBag diagnostic)
        {
            switch (node.Kind)
            {
                case SyntaxNodeKind.EmptySyntax:
                    this.GenerateEmptySyntax(node, diagnostic);
                    break;
            }
        }

        private void GenerateEmptySyntax(SyntaxNode node, DiagnosticBag diagnostic)
        {
            AssemblyBlock.AddInstruction(new Instruction(null, "nop", null, null));
        }
    }
}