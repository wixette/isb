// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;

using ISB.Parsing;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Runtime
{
    public sealed class AssemblyGenerator
    {
        private Environment environment;
        private readonly string moduleName;
        private readonly int sequenceBase;

        public Assembly AssemblyBlock { get; private set; }

        public AssemblyGenerator(Environment environment,
            SyntaxNode treeRoot,
            string moduleName,
            int startInstructionNo = 0)
        {
            this.environment = environment;
            this.moduleName = moduleName;
            this.sequenceBase = Math.Max(0, startInstructionNo);
            this.AssemblyBlock = new Assembly();

            DiagnosticBag diagnostics = new DiagnosticBag();
            this.Generate(treeRoot, diagnostics);
        }

        private int CurrentIndex { get => this.sequenceBase + this.AssemblyBlock.InstructionSequence.Count; }

        private void Generate(SyntaxNode node, DiagnosticBag diagnostic)
        {
            switch (node.Kind)
            {
                case SyntaxNodeKind.StatementBlockSyntax:
                    foreach (var child in node.Children)
                    {
                        this.Generate(child, diagnostic);
                    }
                    break;
                case SyntaxNodeKind.EmptySyntax:
                    this.GenerateEmptySyntax();
                    break;
                case SyntaxNodeKind.LabelStatementSyntax:
                    this.GenerateLabelSyntax(node.Children[0].Terminator, diagnostic);
                    break;
            }
        }

        private void GenerateEmptySyntax()
        {
            AssemblyBlock.AddInstruction(new Instruction(null, "nop", null, null));
        }

        private void GenerateLabelSyntax(Token labelToken, DiagnosticBag diagnostic)
        {
            if (this.environment.LabelDictionary.ContainsKey(labelToken.Text))
            {
                diagnostic.ReportTwoLabelsWithTheSameName(labelToken.Range, labelToken.Text);
            }
            else
            {
                AssemblyBlock.AddInstruction(new Instruction(labelToken.Text, "nop", null, null));
                this.environment.LabelDictionary.Add(labelToken.Text, CurrentIndex);
            }
        }
    }
}