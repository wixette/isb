// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Diagnostics;

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
        private int labelCounter = 0; // No need to be thread-safe.

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

        private string NewLabel()
        {
            return String.Format("__{0}_{1}__", this.moduleName, this.labelCounter++);
        }

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
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax:
                case SyntaxNodeKind.ParenthesisExpressionSyntax:
                case SyntaxNodeKind.IdentifierExpressionSyntax:
                case SyntaxNodeKind.NumberLiteralExpressionSyntax:
                case SyntaxNodeKind.StringLiteralExpressionSyntax:
                case SyntaxNodeKind.InvocationExpressionSyntax:
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                case SyntaxNodeKind.ArrayAccessExpressionSyntax:
                    this.GenerateExpressionSyntax(node);
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

        private void GenerateSubModuleStatementSyntax(Token name, SyntaxNode body)
        {
            if (this.environment.SubModuleNameDictionary.ContainsKey(name.Text))
            {
                this.diagnostics.ReportTwoSubModulesWithTheSameName(name.Range, name.Text);
            }
            else
            {
                string subLabel = this.NewLabel();
                string endSubLabel = this.NewLabel();
                this.AssemblyBlock.AddInstruction(new Instruction(null, "br", new StringValue(endSubLabel), null));
                this.AssemblyBlock.AddInstruction(new Instruction(subLabel, "nop", null, null));
                this.environment.SubModuleNameDictionary.Add(name.Text, this.CurrentIndex);
                this.Generate(body);
                this.AssemblyBlock.AddInstruction(new Instruction(null, "ret", null, null));
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

        private void GenerateExpressionSyntax(SyntaxNode node)
        {
            switch (node.Kind)
            {
                case SyntaxNodeKind.UnaryOperatorExpressionSyntax:
                    this.GenerateUnaryOperatorExpressionSyntax(
                        node.Children[0].Terminator, node.Children[1]);
                    break;
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax:
                    this.GenerateBinaryOperatorExpressionSyntax(
                        node.Children[1].Terminator, node.Children[0], node.Children[2]);
                    break;
                case SyntaxNodeKind.ParenthesisExpressionSyntax:
                    break;
                case SyntaxNodeKind.IdentifierExpressionSyntax:
                    GenerateIdentifierExpressionSyntax(node.Terminator);
                    break;
                case SyntaxNodeKind.NumberLiteralExpressionSyntax:
                    GeneranteNumberLiteralExpressionSyntax(node.Terminator);
                    break;
                case SyntaxNodeKind.StringLiteralExpressionSyntax:
                    GeneranteStringLiteralExpressionSyntax(node.Terminator);
                    break;
                case SyntaxNodeKind.InvocationExpressionSyntax:
                    break;
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                    break;
                case SyntaxNodeKind.ArrayAccessExpressionSyntax:
                    break;
            }
        }

        private void GenerateIdentifierExpressionSyntax(Token identifier)
        {
            this.AssemblyBlock.AddInstruction(new Instruction(null, "load", new StringValue(identifier.Text), null));
        }

        private void GeneranteNumberLiteralExpressionSyntax(Token number)
        {
            BaseValue value = StringValue.Parse(number.Text);
            Debug.Assert(value is NumberValue);
            this.AssemblyBlock.AddInstruction(new Instruction(null, "push", value, null));
        }

        private void GeneranteStringLiteralExpressionSyntax(Token str)
        {
            BaseValue value = StringValue.Parse(str.Text);
            Debug.Assert(value is StringValue);
            this.AssemblyBlock.AddInstruction(new Instruction(null, "push", value, null));
        }

        private void GenerateUnaryOperatorExpressionSyntax(Token op, SyntaxNode expression)
        {
            switch (op.Kind)
            {
                case TokenKind.Minus:
                    // Same as (0 - exp).
                    this.AssemblyBlock.AddInstruction(new Instruction(null, "push", new NumberValue(0), null));
                    this.GenerateExpressionSyntax(expression);
                    this.AssemblyBlock.AddInstruction(new Instruction(null, "sub", null, null));
                    break;
                default:
                    Debug.Fail("Unkonwn unary operator.");
                    break;
            }
        }

        private void GenerateBinaryOperatorExpressionSyntax(Token op, SyntaxNode left, SyntaxNode right)
        {
            if (op.Kind == TokenKind.And)
            {
                string labelLeftIsTrue = this.NewLabel();
                string labelResultIsTrue = this.NewLabel();
                string labelResultIsFalse = this.NewLabel();
                string labelDone = this.NewLabel();

                this.GenerateExpressionSyntax(left);
                this.AssemblyBlock.AddInstruction(
                    new Instruction(null, "br_if", new StringValue(labelLeftIsTrue), new StringValue(labelResultIsFalse)));
                this.AssemblyBlock.AddInstruction(new Instruction(labelLeftIsTrue, "nop", null, null));
                this.GenerateExpressionSyntax(right);
                this.AssemblyBlock.AddInstruction(
                    new Instruction(null, "br_if", new StringValue(labelResultIsTrue), new StringValue(labelResultIsFalse)));
                this.AssemblyBlock.AddInstruction(new Instruction(labelResultIsTrue, "nop", null, null));
                this.AssemblyBlock.AddInstruction(new Instruction(null, "push", new BooleanValue(true), null));
                this.AssemblyBlock.AddInstruction(new Instruction(null, "br", new StringValue(labelDone), null));
                this.AssemblyBlock.AddInstruction(new Instruction(labelResultIsFalse, "nop", null, null));
                this.AssemblyBlock.AddInstruction(new Instruction(null, "push", new BooleanValue(false), null));
                this.AssemblyBlock.AddInstruction(new Instruction(labelDone, "nop", null, null));
            }
            else if (op.Kind == TokenKind.Or)
            {

            }
            else
            {
                // Evaluates and pushes left oprand first.
                this.GenerateExpressionSyntax(left);
                this.GenerateExpressionSyntax(right);
                string instructionName = null;
                switch (op.Kind)
                {
                    case TokenKind.Plus:
                        instructionName = "add";
                        break;
                    case TokenKind.Minus:
                        instructionName = "sub";
                        break;
                    case TokenKind.Multiply:
                        instructionName = "mul";
                        break;
                    case TokenKind.Divide:
                        instructionName = "div";
                        break;
                    case TokenKind.Equal:
                        instructionName = "eq"; // TODO: Distinguish equal and assignment.
                        break;
                    case TokenKind.NotEqual:
                        instructionName = "ne";
                        break;
                    case TokenKind.LessThan:
                        instructionName = "lt";
                        break;
                    case TokenKind.GreaterThan:
                        instructionName = "gt";
                        break;
                    case TokenKind.LessThanOrEqual:
                        instructionName = "le";
                        break;
                    case TokenKind.GreaterThanOrEqual:
                        instructionName = "ge";
                        break;
                    default:
                        Debug.Fail("Unkonwn binary operator.");
                        break;
                }
                this.AssemblyBlock.AddInstruction(new Instruction(null, instructionName, null, null));
            }
        }
    }
}