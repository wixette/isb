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
        private readonly Environment environment;
        private readonly string moduleName;
        private readonly DiagnosticBag diagnostics;
        private int labelCounter = 0; // No need to be thread-safe.

        public Assembly Instructions { get; private set; }

        public AssemblyGenerator(Environment environment, string moduleName, DiagnosticBag diagnostics)
        {
            this.environment = environment;
            this.moduleName = moduleName;
            this.diagnostics = diagnostics;
            this.Instructions = new Assembly();
            this.Reset();
        }

        public void Reset()
        {
            this.environment.Reset();
            this.Instructions.Clear();
        }

        public void Generate(SyntaxNode syntaxTree)
        {
            this.GenerateSyntax(syntaxTree);
        }

        private string NewLabel()
        {
            return String.Format("__{0}_{1}__", this.moduleName, this.labelCounter++);
        }

        private void GenerateSyntax(SyntaxNode node)
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
                    // TODO
                    break;
                case SyntaxNodeKind.WhileStatementSyntax:
                    // TODO
                    break;
                case SyntaxNodeKind.ForStatementSyntax:
                    // TODO
                    break;

                case SyntaxNodeKind.ExpressionStatementSyntax:
                    this.GenerateExpressionStatementSyntax(node);
                    break;
            }
        }

        private void GenerateStatementBlockSyntax(SyntaxNode node)
        {
            foreach (var child in node.Children)
            {
                this.GenerateSyntax(child);
            }
        }

        private void GenerateSubModuleStatementSyntax(Token name, SyntaxNode body)
        {
            if (this.environment.SubModuleNameDictionary.ContainsKey(name.Text))
            {
                this.diagnostics.ReportTwoSubModulesWithTheSameName(name.Range, name.Text);
                return;
            }
            // Sub modules are implemented as 0-argument 0-return functions.
            string subLabel = this.NewLabel();
            string endSubLabel = this.NewLabel();
            this.Instructions.Add(null, Instruction.BR, endSubLabel, null);
            this.Instructions.Add(subLabel, Instruction.NOP, null, null);
            this.environment.SubModuleNameDictionary.Add(name.Text, this.Instructions.Count);
            this.GenerateSyntax(body);
            this.Instructions.Add(null, Instruction.RET, "0", null);
            this.Instructions.Add(endSubLabel, Instruction.NOP, null, null);
        }

        private void GenerateLabelSyntax(Token label)
        {
            if (this.environment.LabelDictionary.ContainsKey(label.Text))
            {
                this.diagnostics.ReportTwoLabelsWithTheSameName(label.Range, label.Text);
            }
            else
            {
                this.Instructions.Add(label.Text, Instruction.NOP, null, null);
                this.environment.LabelDictionary.Add(label.Text, this.Instructions.Count);
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
                this.Instructions.Add(null, Instruction.BR, label.Text, null);
            }
        }

        private void GenerateExpressionStatementSyntax(SyntaxNode node)
        {
            this.GenerateExpressionSyntax(node.Children[0], true);
        }

        private void GenerateExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            switch (node.Kind)
            {
                case SyntaxNodeKind.UnaryOperatorExpressionSyntax:
                    this.GenerateUnaryOperatorExpressionSyntax(
                        node.Children[0].Terminator, node.Children[1], inExpressionStatement);
                    break;
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax:
                    this.GenerateBinaryOperatorExpressionSyntax(
                        node.Children[1].Terminator, node.Children[0], node.Children[2], inExpressionStatement);
                    break;
                case SyntaxNodeKind.ParenthesisExpressionSyntax:
                    this.GenerateExpressionSyntax(node.Children[1], inExpressionStatement);
                    break;
                case SyntaxNodeKind.IdentifierExpressionSyntax:
                    this.GenerateIdentifierExpressionSyntax(node.Terminator);
                    break;
                case SyntaxNodeKind.NumberLiteralExpressionSyntax:
                    this.GeneranteNumberLiteralExpressionSyntax(node.Terminator);
                    break;
                case SyntaxNodeKind.StringLiteralExpressionSyntax:
                    this.GeneranteStringLiteralExpressionSyntax(node.Terminator);
                    break;
                case SyntaxNodeKind.InvocationExpressionSyntax:
                    this.GenerateInvocationExpressionSyntax(node, inExpressionStatement);
                    break;
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                    this.GenerateObjectAccessExpressionSyntax(node, inExpressionStatement, false);
                    break;
                case SyntaxNodeKind.ArrayAccessExpressionSyntax:
                    this.GenerateArrayAccessExpressionSyntax(node, inExpressionStatement, false);
                    break;
            }
        }

        private void GenerateIdentifierExpressionSyntax(Token identifier)
        {
            this.Instructions.Add(null, Instruction.LOAD, identifier.Text, null);
        }

        private void GeneranteNumberLiteralExpressionSyntax(Token number)
        {
            this.Instructions.Add(null, Instruction.PUSH, number.Text, null);
        }

        private void GeneranteStringLiteralExpressionSyntax(Token str)
        {
            // Removes the leading and trailing quotes that the scanner has kept.
            string s = str.Text.Trim('"');
            this.Instructions.Add(null, Instruction.PUSHS, s, null);
        }

        private void GenerateUnaryOperatorExpressionSyntax(
            Token op, SyntaxNode expression, bool inExpressionStatement)
        {
            switch (op.Kind)
            {
                case TokenKind.Minus:
                    // Same as (0 - exp).
                    this.Instructions.Add(null, Instruction.PUSH, Instruction.ZeroLiteral, null);
                    this.GenerateExpressionSyntax(expression, inExpressionStatement);
                    this.Instructions.Add(null, Instruction.SUB, null, null);
                    break;
                default:
                    Debug.Fail("Unkonwn unary operator.");
                    break;
            }
        }

        private void GenerateBinaryOperatorExpressionSyntax(
            Token op, SyntaxNode left, SyntaxNode right, bool inExpressionStatement)
        {
            if (op.Kind == TokenKind.And)
            {
                this.GenerateLogicalAndOperatorExpressionSyntax(left, right, inExpressionStatement);
            }
            else if (op.Kind == TokenKind.Or)
            {
                this.GenerateLogicalOrOperatorExpressionSyntax(left, right, inExpressionStatement);
            }
            else if (op.Kind == TokenKind.Equal && inExpressionStatement)
            {
                // The "=" token is always treated as the assignment operator in expression statements.
                // Otherwise, it is treated as the logical equal operator. Mixed cases like
                //
                //   a = (a = 3)
                //
                // as standalone statements are not supported. while cases like
                //
                //   while a = 3 and b = 4
                //   endwhile
                //
                // are supported.
                if (left.Kind == SyntaxNodeKind.IdentifierExpressionSyntax)
                {
                    string variableName = left.Terminator.Text;
                    this.GenerateAssgnToVariableExpressionSyntax(variableName, right, inExpressionStatement);
                }
                else if (left.Kind == SyntaxNodeKind.ArrayAccessExpressionSyntax)
                {
                    this.GenerateAssignToArrayItemExpressionSyntax(left, right, inExpressionStatement);
                }
                else if (left.Kind == SyntaxNodeKind.ObjectAccessExpressionSyntax)
                {
                    this.GenerateAssignToLibPropertyExpressionSyntax(left, right, inExpressionStatement);
                }
                else
                {
                    // The left expression cannot result in a valid left value.
                    this.diagnostics.ReportExpectedALeftValue(left.Range);
                }
            }
            else
            {
                // Evaluates and pushes left oprand then right oprand.
                this.GenerateExpressionSyntax(left, inExpressionStatement);
                this.GenerateExpressionSyntax(right, inExpressionStatement);
                string instructionName = null;
                switch (op.Kind)
                {
                    case TokenKind.Plus:
                        instructionName = Instruction.ADD;
                        break;
                    case TokenKind.Minus:
                        instructionName = Instruction.SUB;
                        break;
                    case TokenKind.Multiply:
                        instructionName = Instruction.MUL;
                        break;
                    case TokenKind.Divide:
                        instructionName = Instruction.DIV;
                        break;
                    case TokenKind.Equal:
                        instructionName = Instruction.EQ;
                        break;
                    case TokenKind.NotEqual:
                        instructionName = Instruction.NE;
                        break;
                    case TokenKind.LessThan:
                        instructionName = Instruction.LT;
                        break;
                    case TokenKind.GreaterThan:
                        instructionName = Instruction.GT;
                        break;
                    case TokenKind.LessThanOrEqual:
                        instructionName = Instruction.LE;
                        break;
                    case TokenKind.GreaterThanOrEqual:
                        instructionName = Instruction.GE;
                        break;
                    default:
                        Debug.Fail("Unkonwn binary operator.");
                        break;
                }
                this.Instructions.Add(null, instructionName, null, null);
            }
        }

        private void GenerateLogicalAndOperatorExpressionSyntax(
            SyntaxNode left, SyntaxNode right, bool inExpressionStatement)
        {
            string labelLeftIsTrue = this.NewLabel();
            string labelResultIsTrue = this.NewLabel();
            string labelResultIsFalse = this.NewLabel();
            string labelDone = this.NewLabel();

            this.GenerateExpressionSyntax(left, inExpressionStatement);
            this.Instructions.Add(null, Instruction.BR_IF, labelLeftIsTrue, labelResultIsFalse);
            this.Instructions.Add(labelLeftIsTrue, Instruction.NOP, null, null);
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.Instructions.Add(null, Instruction.BR_IF, labelResultIsTrue, labelResultIsFalse);
            this.Instructions.Add(labelResultIsTrue, Instruction.NOP, null, null);
            this.Instructions.Add(null, Instruction.PUSH, Instruction.TrueLiteral, null);
            this.Instructions.Add(null, Instruction.BR, labelDone, null);
            this.Instructions.Add(labelResultIsFalse, Instruction.NOP, null, null);
            this.Instructions.Add(null, Instruction.PUSH, Instruction.FalseLiteral, null);
            this.Instructions.Add(labelDone, Instruction.NOP, null, null);
        }

        private void GenerateLogicalOrOperatorExpressionSyntax(
            SyntaxNode left, SyntaxNode right, bool inExpressionStatement)
        {
            string labelLeftIsFalse = this.NewLabel();
            string labelResultIsTrue = this.NewLabel();
            string labelResultIsFalse = this.NewLabel();
            string labelDone = this.NewLabel();

            this.GenerateExpressionSyntax(left, inExpressionStatement);
            this.Instructions.Add(null, Instruction.BR_IF, labelResultIsTrue, labelLeftIsFalse);
            this.Instructions.Add(labelLeftIsFalse, Instruction.NOP, null, null);
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.Instructions.Add(null, Instruction.BR_IF, labelResultIsTrue, labelResultIsFalse);
            this.Instructions.Add(labelResultIsTrue, Instruction.NOP, null, null);
            this.Instructions.Add(null, Instruction.PUSH, Instruction.TrueLiteral, null);
            this.Instructions.Add(null, Instruction.BR, labelDone, null);
            this.Instructions.Add(labelResultIsFalse, Instruction.NOP, null, null);
            this.Instructions.Add(null, Instruction.PUSH, Instruction.FalseLiteral, null);
            this.Instructions.Add(labelDone, Instruction.NOP, null, null);
        }

        private void GenerateAssgnToVariableExpressionSyntax(
            string variableName, SyntaxNode right, bool inExpressionStatement)
        {
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.Instructions.Add(null, Instruction.STORE, variableName, null);
        }

        private void GenerateAssignToArrayItemExpressionSyntax(
            SyntaxNode left, SyntaxNode right, bool inExpressionStatement)
        {
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.GenerateArrayAccessExpressionSyntax(left, inExpressionStatement, true);
        }

        private void GenerateAssignToLibPropertyExpressionSyntax(
            SyntaxNode left, SyntaxNode right, bool inExpressionStatement)
        {
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.GenerateObjectAccessExpressionSyntax(left, inExpressionStatement, true);
        }

        private void GenerateArrayAccessExpressionSyntax(
            SyntaxNode node, bool inExpressionStatement, bool forLeftValue)
        {
            var (dimension, arrayName) = GenerateArrayIndex(node, inExpressionStatement);
            if (forLeftValue)
            {
                this.Instructions.Add(null, Instruction.STORE_ARR, arrayName, dimension.ToString());
            }
            else
            {
                this.Instructions.Add(null, Instruction.LOAD_ARR, arrayName, dimension.ToString());
            }
        }

        private (int, string) GenerateArrayIndex(SyntaxNode node, bool inExpressionStatement)
        {
            this.GenerateExpressionSyntax(node.Children[2], inExpressionStatement);
            if (node.Children[0].IsTerminator)
            {
                return (1, node.Children[0].Terminator.Text);
            }
            else
            {
                var (dimension, arrayName) = GenerateArrayIndex(node.Children[0], inExpressionStatement);
                return (1 + dimension, arrayName);
            }
        }

        private (string, string) GetLibNameAndMemberName(SyntaxNode objectAccessNode)
        {
            string libName = objectAccessNode.Children[0].Terminator.Text;
            string memberName = objectAccessNode.Children[2].Terminator.Text;
            return (libName, memberName);
        }

        private void GenerateObjectAccessExpressionSyntax(
            SyntaxNode node, bool inExpressionStatement, bool forLeftValue)
        {
            if (node.Children[0].Kind != SyntaxNodeKind.IdentifierExpressionSyntax)
            {
                // Embeded ObjectAccessExpressionSyntax like "a.b.c = 0" is not supported.
                this.diagnostics.ReportUnsupportedDotBaseExpression(node.Range);
                return;
            }
            var (libName, propertyName) = this.GetLibNameAndMemberName(node);
            if (forLeftValue)
            {
                if (!this.environment.Libs.IsPropertyWritable(libName, propertyName))
                {
                    this.diagnostics.ReportPropertyHasNoSetter(node.Range, libName, propertyName);
                    return;
                }
                this.Instructions.Add(null, Instruction.STORE_LIB, libName, propertyName);
            }
            else
            {
                if (!this.environment.Libs.IsPropertyExist(libName, propertyName))
                {
                    this.diagnostics.ReportLibraryMemberNotFound(node.Range, libName, propertyName);
                    return;
                }
                this.Instructions.Add(null, Instruction.LOAD_LIB, libName, propertyName);
            }
        }

        private void GenerateInvocationExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            switch (node.Children[0].Kind)
            {
                case SyntaxNodeKind.IdentifierExpressionSyntax:
                    string subName = node.Children[0].Terminator.Text;
                    if (!this.environment.SubModuleNameDictionary.ContainsKey(subName))
                    {
                        // TODO:
                        //  (1) forwardly check if the sub name is defined?
                        //  (2) a separate diagnostic code for NoSubModuleDefined?
                        this.diagnostics.ReportUnsupportedInvocationBaseExpression(node.Range);
                        return;
                    }
                    GenerateCallSub(subName, node.Children[2]);
                    break;
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                    GenerateCallLib(node.Children[0], node.Children[2], inExpressionStatement);
                    break;
                default:
                    // Embedded InvocationExpressionSyntax like "a()()" is not supported.
                    this.diagnostics.ReportUnsupportedInvocationBaseExpression(node.Range);
                    break;
            }
        }

        private void GenerateCallSub(string subName, SyntaxNode argumentGroupNode)
        {
            if (!argumentGroupNode.IsEmpty)
            {
                // No argument for sub module is supported.
                this.diagnostics.ReportUnexpectedArgumentsCount(
                    argumentGroupNode.Range, argumentGroupNode.Children.Count, 0);
                return;
            }
            this.Instructions.Add(null, Instruction.CALL, subName, null);
        }

        private void GenerateCallLib(
            SyntaxNode objectAccessNode, SyntaxNode argumentGroupNode, bool inExpressionStatement)
        {
            int argumentNumber = 0;
            if (argumentGroupNode.Kind == SyntaxNodeKind.ArgumentGroupSyntax)
            {
                argumentNumber = argumentGroupNode.Children.Count;
            }
            // Pushes arguments in reversed order.
            for (int i = argumentGroupNode.Children.Count - 1; i >= 0; i--)
            {
                SyntaxNode argumentExpression = argumentGroupNode.Children[i].Children[0];
                GenerateExpressionSyntax(argumentExpression, inExpressionStatement);
            }
            if (objectAccessNode.Children[0].Kind != SyntaxNodeKind.IdentifierExpressionSyntax)
            {
                // Embeded ObjectAccessExpressionSyntax like "a.b.c()" is not supported.
                this.diagnostics.ReportUnsupportedDotBaseExpression(objectAccessNode.Range);
                return;
            }
            var (libName, funcName) = this.GetLibNameAndMemberName(objectAccessNode);
            if (!this.environment.Libs.IsFuncExist(libName, funcName))
            {
                // TODO: a separate diagnostic code for NoLibFuncDefined?
                this.diagnostics.ReportUnsupportedInvocationBaseExpression(objectAccessNode.Range);
                return;
            }
            int expectedArgumentNumber = this.environment.Libs.GetFuncArgumentNumber(libName, funcName);
            if (argumentNumber != expectedArgumentNumber)
            {
                // TODO: report this error once the Libraries class is ready.
                // this.diagnostics.ReportUnexpectedArgumentsCount(
                //    objectAccessNode.Range, argumentNumber, expectedArgumentNumber);
            }
            this.Instructions.Add(null, Instruction.CALL_LIB, libName, funcName);
        }
    }
}