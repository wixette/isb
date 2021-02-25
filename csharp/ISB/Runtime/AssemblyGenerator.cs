// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;

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
        private readonly DiagnosticBag diagnostics;

        public Assembly AssemblyBlock { get; private set; }

        public AssemblyGenerator(Environment environment,
            SyntaxNode treeRoot,
            string moduleName,
            int sequenceBase,
            DiagnosticBag diagnostics)
        {
            this.environment = environment;
            this.moduleName = moduleName;
            this.sequenceBase = Math.Max(0, sequenceBase);
            this.diagnostics = diagnostics;
            this.AssemblyBlock = new Assembly();

            this.Generate(treeRoot);
        }

        private int CurrentIndex { get => this.sequenceBase + this.AssemblyBlock.InstructionSequence.Count; }

        private void Generate(SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxNodeKind.EmptySyntax:
                case SyntaxNodeKind.CommentStatementSyntax:
                case SyntaxNodeKind.UnrecognizedStatementSyntax:
                case SyntaxNodeKind.UnrecognizedExpressionSyntax:
                    break;

                case SyntaxNodeKind.StatementBlockSyntax:
                    this.GenerateStatementBlockSyntax(node);
                    break;

                case SyntaxNodeKind.SubModuleStatementSyntax:
                    this.GenerateSubModuleStatementSyntax(node.Children[1].Terminator, node.Children[2]);
                    break;

                case SyntaxNodeKind.LabelStatementSyntax:
                    this.GenerateLabelSyntax(node.Children[0].Terminator);
                    break;
                case SyntaxNodeKind.GoToStatementSyntax:
                    this.GenerateGotoSyntax(node.Children[1].Terminator);
                    break;

                case SyntaxNodeKind.IfStatementSyntax:
                    break;
                case SyntaxNodeKind.WhileStatementSyntax:
                    break;
                case SyntaxNodeKind.ForStatementSyntax:
                    break;

                case SyntaxNodeKind.UnaryOperatorExpressionSyntax:
                    break;
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax:
                    break;
                case SyntaxNodeKind.ParenthesisExpressionSyntax:
                    break;

                case SyntaxNodeKind.IdentifierExpressionSyntax:
                    break;
                case SyntaxNodeKind.NumberLiteralExpressionSyntax:
                    break;
                case SyntaxNodeKind.StringLiteralExpressionSyntax:
                    break;

                case SyntaxNodeKind.InvocationExpressionSyntax:
                    break;
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                    break;
                case SyntaxNodeKind.ArrayAccessExpressionSyntax:
                    break;
            }
        }

        private void GenerateStatementBlockSyntax(SyntaxNode node)
        {
            foreach (var child in node.Children)
            {
                this.Generate(child);
            }
        }

        private string GetSubLabel(string name)
            => String.Format("__{0}_sub_{1}__", this.moduleName, name);

        private string GetEndSubLabel(string name)
            => String.Format("__{0}_endsub_{1}__", this.moduleName, name);

        private void GenerateSubModuleStatementSyntax(Token name, SyntaxNode body)
        {
            if (this.environment.SubModuleNameDictionary.ContainsKey(name.Text))
            {
                this.diagnostics.ReportTwoSubModulesWithTheSameName(name.Range, name.Text);
            }
            else
            {
                string endSubLabel = this.GetEndSubLabel(name.Text);
                this.AssemblyBlock.AddInstruction(new Instruction(null, "br", new StringValue(endSubLabel), null));
                string subLabel = this.GetSubLabel(name.Text);
                this.AssemblyBlock.AddInstruction(new Instruction(subLabel, "nop", null, null));
                this.environment.SubModuleNameDictionary.Add(name.Text, this.CurrentIndex);
                this.Generate(body);
                this.AssemblyBlock.AddInstruction(new Instruction(endSubLabel, "nop", null, null));
            }
        }

        private void GenerateLabelSyntax(Token label)
        {
            if (this.environment.LabelDictionary.ContainsKey(label.Text))
            {
                this.diagnostics.ReportTwoLabelsWithTheSameName(label.Range, label.Text);
            }
            else
            {
                this.AssemblyBlock.AddInstruction(new Instruction(label.Text, "nop", null, null));
                this.environment.LabelDictionary.Add(label.Text, this.CurrentIndex);
            }
        }

        private void GenerateGotoSyntax(Token label)
        {
            if (!this.environment.LabelDictionary.ContainsKey(label.Text))
            {
                this.diagnostics.ReportGoToUndefinedLabel(label.Range, label.Text);
            }
            else
            {
                this.AssemblyBlock.AddInstruction(new Instruction(null, "br", new StringValue(label.Text), null));
            }
        }
    }
}