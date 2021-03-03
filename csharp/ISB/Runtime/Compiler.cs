// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Diagnostics;

using ISB.Parsing;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Runtime
{
    // An incremental (stateful) compiler to translate ISB source into ISB assembly instructions.
    public sealed class Compiler
    {
        private readonly Environment env;
        private readonly string moduleName;
        private readonly DiagnosticBag diagnostics;
        private int labelCounter = 0; // No need to be thread-safe.

        public Assembly Instructions { get; private set; }

        public Compiler(Environment env, string moduleName, DiagnosticBag diagnostics)
        {
            this.env = env;
            this.moduleName = moduleName;
            this.diagnostics = diagnostics;
            this.Instructions = new Assembly();
            this.Reset();
        }

        public void Reset()
        {
            this.env.Reset();
            this.Instructions.Clear();
        }

        public void Compile(SyntaxNode syntaxTree)
        {
            this.GenerateSyntax(syntaxTree);
        }

        public void ParseAssembly(string assemblyCode)
        {
            this.Instructions = Assembly.Parse(assemblyCode);
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
                    this.GenerateIfStatementSyntax(node);
                    break;
                case SyntaxNodeKind.WhileStatementSyntax:
                    this.GenerateWhileStatementSyntax(node);
                    break;
                case SyntaxNodeKind.ForStatementSyntax:
                    this.GenerateForStatementSyntax(node);
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
            if (this.env.SubNames.ContainsKey(name.Text))
            {
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportTwoSubModulesWithTheSameName(name.Range, name.Text));
                return;
            }
            // Sub modules are implemented as 0-argument 0-return functions.
            string subLabel = this.NewLabel();
            string endSubLabel = this.NewLabel();
            this.Instructions.Add(null, Instruction.BR, endSubLabel, null);
            this.Instructions.Add(subLabel, Instruction.NOP, null, null);
            this.env.SubNames.Add(name.Text, this.Instructions.Count - 1);
            this.GenerateSyntax(body);
            this.Instructions.Add(null, Instruction.RET, "0", null);
            this.Instructions.Add(endSubLabel, Instruction.NOP, null, null);
        }

        private void GenerateLabelSyntax(Token label)
        {
            if (this.env.Labels.ContainsKey(label.Text))
            {
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportTwoLabelsWithTheSameName(label.Range, label.Text));
            }
            else
            {
                this.Instructions.Add(label.Text, Instruction.NOP, null, null);
                this.env.Labels.Add(label.Text, this.Instructions.Count - 1);
            }
        }

        private void GenerateGotoSyntax(Token label)
        {
            if (!this.env.Labels.ContainsKey(label.Text))
            {
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportGoToUndefinedLabel(label.Range, label.Text));
            }
            else
            {
                this.Instructions.Add(null, Instruction.BR, label.Text, null);
            }
        }

        private void GenerateExpressionStatementSyntax(SyntaxNode node)
        {
            // There are basically two kinds of expression statements:
            //   (1) Assignment expression statements.
            //   (2) Standalone expression statements.
            //
            // For case (1), if the top most syntax of the expression statement is assignment,
            // the final value of the expression will be consumed by the left value of the expression.
            // Nothing is left on the stack top once the statement is done.
            //
            // For case (2), the final value of the expression will be left on the stack top
            // since no one consumes it.
            //
            // TODO: review this logic when implementing the interactive shell. It could be the
            // shell's duty to consume the value of standalone expressions.
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
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportExpectedALeftValue(left.Range));
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
            var (dimension, arrayName) = this.GenerateArrayIndex(node, inExpressionStatement);
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
                var (dimension, arrayName) = this.GenerateArrayIndex(node.Children[0], inExpressionStatement);
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
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnsupportedDotBaseExpression(node.Range));
                return;
            }
            var (libName, propertyName) = this.GetLibNameAndMemberName(node);
            if (forLeftValue)
            {
                if (!this.env.Libs.IsPropertyWritable(libName, propertyName))
                {
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportPropertyHasNoSetter(node.Range, libName, propertyName));
                    return;
                }
                this.Instructions.Add(null, Instruction.STORE_LIB, libName, propertyName);
            }
            else
            {
                if (!this.env.Libs.IsPropertyExist(libName, propertyName))
                {
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportLibraryMemberNotFound(node.Range, libName, propertyName));
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
                    if (!this.env.SubNames.ContainsKey(subName))
                    {
                        // TODO:
                        //  (1) forwardly check if the sub name is defined?
                        //  (2) a separate diagnostic code for NoSubModuleDefined?
                        if (this.diagnostics != null)
                            this.diagnostics.Add(Diagnostic.ReportUnsupportedInvocationBaseExpression(node.Range));
                        return;
                    }
                    this.GenerateCallSub(subName, node.Children[2]);
                    break;
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                    this.GenerateCallLib(node.Children[0], node.Children[2], inExpressionStatement);
                    break;
                default:
                    // Embedded InvocationExpressionSyntax like "a()()" is not supported.
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportUnsupportedInvocationBaseExpression(node.Range));
                    break;
            }
        }

        private void GenerateCallSub(string subName, SyntaxNode argumentGroupNode)
        {
            if (!argumentGroupNode.IsEmpty)
            {
                // No argument for sub module is supported.
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnexpectedArgumentsCount(
                        argumentGroupNode.Range, argumentGroupNode.Children.Count, 0));
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
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnsupportedDotBaseExpression(objectAccessNode.Range));
                return;
            }
            var (libName, funcName) = this.GetLibNameAndMemberName(objectAccessNode);
            if (!this.env.Libs.IsFuncExist(libName, funcName))
            {
                // TODO: a separate diagnostic code for NoLibFuncDefined?
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnsupportedInvocationBaseExpression(objectAccessNode.Range));
                return;
            }
            int expectedArgumentNumber = this.env.Libs.GetFuncArgumentNumber(libName, funcName);
            if (argumentNumber != expectedArgumentNumber)
            {
                // TODO: report this error once the Libraries class is ready.
                // if (this.diagnostics != null)
                //     this.diagnostics.Add(Diagnostic.ReportUnexpectedArgumentsCount)(
                //        objectAccessNode.Range, argumentNumber, expectedArgumentNumber);
            }
            this.Instructions.Add(null, Instruction.CALL_LIB, libName, funcName);
        }

        private void GenerateIfStatementSyntax(SyntaxNode node)
        {
            SyntaxNode ifPartNode = node.Children[0];
            SyntaxNode ifConditionNode = ifPartNode.Children[1];
            SyntaxNode ifBodyNode = ifPartNode.Children[3];
            string endLabel = this.NewLabel();

            this.GenerateIfConditionalBlock(ifConditionNode, ifBodyNode, endLabel);

            SyntaxNode elseIfPartGroupNode = node.Children[1];
            if (!elseIfPartGroupNode.IsEmpty)
            {
                foreach (var elseIfPartNode in elseIfPartGroupNode.Children)
                {
                    var conditionNode = elseIfPartNode.Children[1];
                    var bodyNode = elseIfPartNode.Children[3];
                    this.GenerateIfConditionalBlock(conditionNode, bodyNode, endLabel);
                }
            }

            SyntaxNode elsePartNode = node.Children[2];
            if (!elsePartNode.IsEmpty)
            {
                SyntaxNode elseBodyNode = elsePartNode.Children[1];
                this.GenerateStatementBlockSyntax(elseBodyNode);
            }

            this.Instructions.Add(endLabel, Instruction.NOP, null, null);
        }

        private void GenerateIfConditionalBlock(SyntaxNode conditionNode, SyntaxNode bodyNode, string endLabel)
        {
            string trueLabel = this.NewLabel();
            string falseLabel = this.NewLabel();
            this.GenerateExpressionSyntax(conditionNode, false);
            this.Instructions.Add(null, Instruction.BR_IF, trueLabel, falseLabel);
            this.Instructions.Add(trueLabel, Instruction.NOP, null, null);
            this.GenerateStatementBlockSyntax(bodyNode);
            this.Instructions.Add(null, Instruction.BR, endLabel, null);
            this.Instructions.Add(falseLabel, Instruction.NOP, null, null);
        }

        private void GenerateWhileStatementSyntax(SyntaxNode node)
        {
            string startLabel = this.NewLabel();
            string bodyLabel = this.NewLabel();
            string endLabel = this.NewLabel();
            this.Instructions.Add(startLabel, Instruction.NOP, null, null);

            SyntaxNode conditionNode = node.Children[1];
            this.GenerateExpressionSyntax(conditionNode, false);
            this.Instructions.Add(null, Instruction.BR_IF, bodyLabel, endLabel);
            this.Instructions.Add(bodyLabel, Instruction.NOP, null, null);

            SyntaxNode bodyNode = node.Children[2];
            this.GenerateStatementBlockSyntax(bodyNode);
            this.Instructions.Add(null, Instruction.BR, startLabel, null);

            this.Instructions.Add(endLabel, Instruction.NOP, null, null);
        }

        private void GenerateForStatementSyntax(SyntaxNode node)
        {
            string loopVariableName = node.Children[1].Terminator.Text;
            SyntaxNode fromExpressionNode = node.Children[3];
            SyntaxNode toExpressionNode = node.Children[5];
            SyntaxNode stepExpressionNode = null;
            SyntaxNode stepClauseNode = node.Children[6];
            if (!stepClauseNode.IsEmpty)
                stepExpressionNode = stepClauseNode.Children[1];
            SyntaxNode bodyNode = node.Children[7];

            // Initializes the loop variable.
            this.GenerateExpressionSyntax(fromExpressionNode, false);
            this.Instructions.Add(null, Instruction.STORE, loopVariableName, null);

            string startLabel = this.NewLabel();
            string endLabel = this.NewLabel();

            // Body statements.
            this.Instructions.Add(startLabel, Instruction.NOP, null, null);
            if (!bodyNode.IsEmpty)
            {
                this.GenerateStatementBlockSyntax(bodyNode);
            }

            // Calculates the step value and stores it to register[0].
            if (stepExpressionNode != null)
            {
                this.GenerateExpressionSyntax(stepExpressionNode, false);
                this.Instructions.Add(null, Instruction.SET, "0", null);
            }
            else
            {
                // Default step value is 1.
                this.Instructions.Add(null, Instruction.PUSH, "1", null);
                this.Instructions.Add(null, Instruction.SET, "0", null);
            }

            // Adds the loop variable by the step value.
            this.Instructions.Add(null, Instruction.LOAD, loopVariableName, null);
            this.Instructions.Add(null, Instruction.GET, "0", null);
            this.Instructions.Add(null, Instruction.ADD, null, null);
            this.Instructions.Add(null, Instruction.STORE, loopVariableName, null);

            // If the step value >=0, goto (1), else, goto (2).
            this.Instructions.Add(null, Instruction.GET, "0", null);
            this.Instructions.Add(null, Instruction.PUSH, "0", null);
            this.Instructions.Add(null, Instruction.GE, null, null);
            string leCompareLabel = this.NewLabel();
            string geCompareLabel = this.NewLabel();
            this.Instructions.Add(null, Instruction.BR_IF, leCompareLabel, geCompareLabel);

            // (1) Checks if the loop variabel is less than or equal to the To value.
            // Continues the for loop if ture, breaks the for loop otherwise.
            this.Instructions.Add(leCompareLabel, Instruction.NOP, null, null);
            this.Instructions.Add(null, Instruction.LOAD, loopVariableName, null);
            this.GenerateExpressionSyntax(toExpressionNode, false);
            this.Instructions.Add(null, Instruction.LE, null, null);
            this.Instructions.Add(null, Instruction.BR_IF, startLabel, endLabel);

            // (2) Checks if the loop variabel is greater than or equal to the To value.
            // Continues the for loop if ture, breaks the for loop otherwise.
            this.Instructions.Add(geCompareLabel, Instruction.NOP, null, null);
            this.Instructions.Add(null, Instruction.LOAD, loopVariableName, null);
            this.GenerateExpressionSyntax(toExpressionNode, false);
            this.Instructions.Add(null, Instruction.GE, null, null);
            this.Instructions.Add(null, Instruction.BR_IF, startLabel, endLabel);

            // The end.
            this.Instructions.Add(endLabel, Instruction.NOP, null, null);
        }
    }
}