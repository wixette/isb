// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;

using ISB.Parsing;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Runtime
{
    // A stateless class that supports both one-time compilations and incremental compilations.
    public sealed class Compiler
    {
        // Compile() can be invoked for incremental compilation. The source code segments can be passed in one by
        // one. It's caller's duty to merge the result assembly and the generated symbol table.
        //
        // An env object is passed in to provide the context info of the previous compiled code segments.
        //
        // (1) env.CompileTimeLabels and env.CompileTimeSubNames are used to check existing symbols.
        // (2) env.NextAutoLabelNo is used as a sequence no generator, for generating auto labels.
        public static Assembly Compile(SyntaxNode syntaxTree,
            Environment env,
            string moduleName,
            DiagnosticBag diagnostics,
            out HashSet<string> newLabels,
            out HashSet<string> newSubNames)
        {
            Assembly assembly = new Assembly();
            newLabels = new HashSet<string>();
            newSubNames = new HashSet<string>();
            Compiler compiler = new Compiler(env, moduleName, diagnostics, assembly, newLabels, newSubNames);
            compiler.CollectLabelsAndSubNames(syntaxTree);
            compiler.GenerateSyntax(syntaxTree);
            return compiler.assembly;
        }

        // Parsing from assembly text format code doesn't generate compile-time labels or compile-time subnames.
        // Only the result assembly is returned.
        public static Assembly ParseAssembly(string assemblyCode)
        {
            Assembly assembly = Assembly.Parse(assemblyCode);
            return assembly;
        }

        private readonly Environment env;
        private readonly string moduleName;
        private readonly DiagnosticBag diagnostics;
        private Assembly assembly;
        private HashSet<string> newLabels;
        private HashSet<string> newSubNames;

        private Compiler(Environment env,
            string moduleName,
            DiagnosticBag diagnostics,
            Assembly assembly,
            HashSet<string> newLabels,
            HashSet<string> newSubNames)
        {
            this.env = env;
            this.moduleName = moduleName;
            this.diagnostics = diagnostics;
            this.assembly = assembly;
            this.newLabels = newLabels;
            this.newSubNames = newSubNames;
        }

        private bool HasLabel(string label)
        {
            return this.env.CompileTimeLabels.Contains(label) || this.newLabels.Contains(label);
        }

        private bool HasSubName(string name)
        {
            return this.env.CompileTimeSubNames.Contains(name) || this.newSubNames.Contains(name);
        }

        private void CollectLabelsAndSubNames(SyntaxNode node)
        {
            LabelsAndSubNamesVisitor visitor = new LabelsAndSubNamesVisitor(this);
            SyntaxTreeWalker walker = new SyntaxTreeWalker(node);
            walker.Walk(visitor, new SyntaxNodeKind[] {
                SyntaxNodeKind.LabelStatementSyntax,
                SyntaxNodeKind.SubModuleStatementSyntax
            });
        }

        private class LabelsAndSubNamesVisitor : ISyntaxNodeVisitor
        {
            private Compiler compiler;

            public LabelsAndSubNamesVisitor(Compiler compiler)
            {
                this.compiler = compiler;
            }

            public void VisitNode(SyntaxNode node, int level)
            {
                if (node.Kind == SyntaxNodeKind.LabelStatementSyntax)
                {
                    Token label = node.Children[0].Terminator;
                    if (this.compiler.HasLabel(label.Text))
                    {
                        if (this.compiler.diagnostics != null)
                            this.compiler.diagnostics.Add(
                                Diagnostic.ReportTwoLabelsWithTheSameName(label.Range, label.Text));
                        return;
                    }
                    this.compiler.newLabels.Add(label.Text);
                }
                else if (node.Kind == SyntaxNodeKind.SubModuleStatementSyntax)
                {
                    Token name = node.Children[1].Terminator;
                    if (this.compiler.HasSubName(name.Text))
                    {
                        if (this.compiler.diagnostics != null)
                            this.compiler.diagnostics.Add(
                                Diagnostic.ReportTwoSubModulesWithTheSameName(name.Range, name.Text));
                        return;
                    }
                    this.compiler.newSubNames.Add(name.Text);
                }
            }
        }

        private string NewLabel()
            => $"__{this.moduleName}_{this.env.NextAutoLabelNo}__";

        private string RuntimeSubLabel(string subName)
            => $"__Sub_{subName}__";

        private void AddInstruction(TextRange sourceRange, string label, string name, string oprand1, string oprand2)
        {
            this.assembly.Add(sourceRange, label, name, oprand1, oprand2);
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
                    this.GenerateSubModuleStatementSyntax(node);
                    break;

                case SyntaxNodeKind.LabelStatementSyntax:
                    this.GenerateLabelSyntax(node);
                    break;
                case SyntaxNodeKind.GoToStatementSyntax:
                    this.GenerateGotoSyntax(node);
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

        private void GenerateSubModuleStatementSyntax(SyntaxNode node)
        {
            Token name = node.Children[1].Terminator;
            SyntaxNode body = node.Children[2];
            // Sub modules are implemented as 0-argument 0-return functions.
            string subLabel = this.RuntimeSubLabel(name.Text);
            string endSubLabel = this.NewLabel();
            this.AddInstruction(node.Range, null, Instruction.BR, endSubLabel, null);
            this.AddInstruction(node.Range, subLabel, Instruction.NOP, null, null);
            this.GenerateSyntax(body);
            this.AddInstruction(node.Range, null, Instruction.RET, "0", null);
            this.AddInstruction(node.Range, endSubLabel, Instruction.NOP, null, null);
        }

        private void GenerateLabelSyntax(SyntaxNode node)
        {
            Token label = node.Children[0].Terminator;
            this.AddInstruction(node.Range, label.Text, Instruction.NOP, null, null);
        }

        private void GenerateGotoSyntax(SyntaxNode node)
        {
            Token label = node.Children[1].Terminator;
            if (!this.HasLabel(label.Text))
            {
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportGoToUndefinedLabel(label.Range, label.Text));
            }
            else
            {
                this.AddInstruction(node.Range, null, Instruction.BR, label.Text, null);
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
            this.GenerateExpressionSyntax(node.Children[0], true);
        }

        private void GenerateExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            switch (node.Kind)
            {
                case SyntaxNodeKind.UnaryOperatorExpressionSyntax:
                    this.GenerateUnaryOperatorExpressionSyntax(node, inExpressionStatement);
                    break;
                case SyntaxNodeKind.BinaryOperatorExpressionSyntax:
                    this.GenerateBinaryOperatorExpressionSyntax(node, inExpressionStatement);
                    break;
                case SyntaxNodeKind.ParenthesisExpressionSyntax:
                    this.GenerateExpressionSyntax(node.Children[1], inExpressionStatement);
                    break;
                case SyntaxNodeKind.IdentifierExpressionSyntax:
                    this.GenerateIdentifierExpressionSyntax(node);
                    break;
                case SyntaxNodeKind.NumberLiteralExpressionSyntax:
                    this.GeneranteNumberLiteralExpressionSyntax(node);
                    break;
                case SyntaxNodeKind.StringLiteralExpressionSyntax:
                    this.GeneranteStringLiteralExpressionSyntax(node);
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

        private void GenerateIdentifierExpressionSyntax(SyntaxNode node)
        {
            Token identifier = node.Terminator;
            this.AddInstruction(node.Range, null, Instruction.LOAD, identifier.Text, null);
        }

        private void GeneranteNumberLiteralExpressionSyntax(SyntaxNode node)
        {
            Token number = node.Terminator;
            this.AddInstruction(node.Range, null, Instruction.PUSH, number.Text, null);
        }

        private void GeneranteStringLiteralExpressionSyntax(SyntaxNode node)
        {
            Token str = node.Terminator;
            // Removes the leading and trailing quotes that the scanner has kept.
            string s = str.Text.Trim('"');
            this.AddInstruction(node.Range, null, Instruction.PUSHS, s, null);
        }

        private void GenerateUnaryOperatorExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            Token op = node.Children[0].Terminator;
            SyntaxNode expression = node.Children[1];
            switch (op.Kind)
            {
                case TokenKind.Minus:
                    // Same as (0 - exp).
                    this.AddInstruction(node.Range, null, Instruction.PUSH, Instruction.ZeroLiteral, null);
                    this.GenerateExpressionSyntax(expression, inExpressionStatement);
                    this.AddInstruction(node.Range, null, Instruction.SUB, null, null);
                    break;
                default:
                    Debug.Fail("Unkonwn unary operator.");
                    break;
            }
        }

        private void GenerateBinaryOperatorExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            Token op = node.Children[1].Terminator;
            SyntaxNode left = node.Children[0];
            SyntaxNode right = node.Children[2];
            if (op.Kind == TokenKind.And)
            {
                this.GenerateLogicalAndOperatorExpressionSyntax(node, inExpressionStatement);
            }
            else if (op.Kind == TokenKind.Or)
            {
                this.GenerateLogicalOrOperatorExpressionSyntax(node, inExpressionStatement);
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
                    this.GenerateAssgnToVariableExpressionSyntax(node, inExpressionStatement);
                }
                else if (left.Kind == SyntaxNodeKind.ArrayAccessExpressionSyntax)
                {
                    this.GenerateAssignToArrayItemExpressionSyntax(node, inExpressionStatement);
                }
                else if (left.Kind == SyntaxNodeKind.ObjectAccessExpressionSyntax)
                {
                    this.GenerateAssignToLibPropertyExpressionSyntax(node, inExpressionStatement);
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
                    case TokenKind.Mod:
                        instructionName = Instruction.MOD;
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
                this.AddInstruction(node.Range, null, instructionName, null, null);
            }
        }

        private void GenerateLogicalAndOperatorExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            SyntaxNode left = node.Children[0];
            SyntaxNode right = node.Children[2];

            string labelLeftIsTrue = this.NewLabel();
            string labelResultIsTrue = this.NewLabel();
            string labelResultIsFalse = this.NewLabel();
            string labelDone = this.NewLabel();

            this.GenerateExpressionSyntax(left, inExpressionStatement);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, labelLeftIsTrue, labelResultIsFalse);
            this.AddInstruction(node.Range, labelLeftIsTrue, Instruction.NOP, null, null);
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, labelResultIsTrue, labelResultIsFalse);
            this.AddInstruction(node.Range, labelResultIsTrue, Instruction.NOP, null, null);
            this.AddInstruction(node.Range, null, Instruction.PUSH, Instruction.TrueLiteral, null);
            this.AddInstruction(node.Range, null, Instruction.BR, labelDone, null);
            this.AddInstruction(node.Range, labelResultIsFalse, Instruction.NOP, null, null);
            this.AddInstruction(node.Range, null, Instruction.PUSH, Instruction.FalseLiteral, null);
            this.AddInstruction(node.Range, labelDone, Instruction.NOP, null, null);
        }

        private void GenerateLogicalOrOperatorExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            SyntaxNode left = node.Children[0];
            SyntaxNode right = node.Children[2];

            string labelLeftIsFalse = this.NewLabel();
            string labelResultIsTrue = this.NewLabel();
            string labelResultIsFalse = this.NewLabel();
            string labelDone = this.NewLabel();

            this.GenerateExpressionSyntax(left, inExpressionStatement);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, labelResultIsTrue, labelLeftIsFalse);
            this.AddInstruction(node.Range, labelLeftIsFalse, Instruction.NOP, null, null);
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, labelResultIsTrue, labelResultIsFalse);
            this.AddInstruction(node.Range, labelResultIsTrue, Instruction.NOP, null, null);
            this.AddInstruction(node.Range, null, Instruction.PUSH, Instruction.TrueLiteral, null);
            this.AddInstruction(node.Range, null, Instruction.BR, labelDone, null);
            this.AddInstruction(node.Range, labelResultIsFalse, Instruction.NOP, null, null);
            this.AddInstruction(node.Range, null, Instruction.PUSH, Instruction.FalseLiteral, null);
            this.AddInstruction(node.Range, labelDone, Instruction.NOP, null, null);
        }

        private void GenerateAssgnToVariableExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            SyntaxNode left = node.Children[0];
            SyntaxNode right = node.Children[2];
            string variableName = left.Terminator.Text;
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.AddInstruction(node.Range, null, Instruction.STORE, variableName, null);
        }

        private void GenerateAssignToArrayItemExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            SyntaxNode left = node.Children[0];
            SyntaxNode right = node.Children[2];
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.GenerateArrayAccessExpressionSyntax(left, inExpressionStatement, true);
        }

        private void GenerateAssignToLibPropertyExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            SyntaxNode left = node.Children[0];
            SyntaxNode right = node.Children[2];
            this.GenerateExpressionSyntax(right, inExpressionStatement);
            this.GenerateObjectAccessExpressionSyntax(left, inExpressionStatement, true);
        }

        private void GenerateArrayAccessExpressionSyntax(SyntaxNode node, bool inExpressionStatement, bool forLeftValue)
        {
            var (dimension, arrayName) = this.GenerateArrayIndex(node, inExpressionStatement);
            if (forLeftValue)
            {
                this.AddInstruction(node.Range, null, Instruction.STORE_ARR, arrayName, dimension.ToString());
            }
            else
            {
                this.AddInstruction(node.Range, null, Instruction.LOAD_ARR, arrayName, dimension.ToString());
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
                this.AddInstruction(node.Range, null, Instruction.STORE_LIB, libName, propertyName);
            }
            else
            {
                if (!this.env.Libs.HasProperty(libName, propertyName))
                {
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportLibraryMemberNotFound(node.Range, libName, propertyName));
                    return;
                }
                this.AddInstruction(node.Range, null, Instruction.LOAD_LIB, libName, propertyName);
            }
        }

        private void GenerateInvocationExpressionSyntax(SyntaxNode node, bool inExpressionStatement)
        {
            switch (node.Children[0].Kind)
            {
                case SyntaxNodeKind.IdentifierExpressionSyntax:
                    string subName = node.Children[0].Terminator.Text;
                    if (this.HasSubName(subName))
                        this.GenerateCallSub(node);
                    else if (this.env.Libs.HasBuiltInFunction(subName))
                        this.GenerateCallLib(node, true, inExpressionStatement);
                    else if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportUnsupportedInvocationBaseExpression(node.Range));
                    break;
                case SyntaxNodeKind.ObjectAccessExpressionSyntax:
                    this.GenerateCallLib(node, false, inExpressionStatement);
                    break;
                default:
                    // Embedded InvocationExpressionSyntax like "a()()" is not supported.
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportUnsupportedInvocationBaseExpression(node.Range));
                    break;
            }
        }

        private void GenerateCallSub(SyntaxNode node)
        {
            string subName = node.Children[0].Terminator.Text;
            SyntaxNode argumentGroupNode = node.Children[2];
            if (!argumentGroupNode.IsEmpty)
            {
                // No argument for sub module is supported.
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnexpectedArgumentsCount(
                        argumentGroupNode.Range, argumentGroupNode.Children.Count, 0));
                return;
            }
            string subLabel = this.RuntimeSubLabel(subName);
            this.AddInstruction(node.Range, null, Instruction.CALL, subLabel, null);
        }

        private void GenerateCallLib(SyntaxNode node, bool isBuiltInFunction, bool inExpressionStatement)
        {
            if (!isBuiltInFunction &&
                node.Children[0].Children[0].Kind != SyntaxNodeKind.IdentifierExpressionSyntax)
            {
                // Embeded ObjectAccessExpressionSyntax like "a.b.c()" is not supported.
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnsupportedDotBaseExpression(node.Children[0].Range));
                return;
            }

            SyntaxNode argumentGroupNode = node.Children[2];
            int argumentNumber = 0;
            if (argumentGroupNode.Kind == SyntaxNodeKind.ArgumentGroupSyntax)
            {
                argumentNumber = argumentGroupNode.Children.Count;
                // Pushes arguments in reversed order.
                for (int i = argumentGroupNode.Children.Count - 1; i >= 0; i--)
                {
                    SyntaxNode argumentExpression = argumentGroupNode.Children[i].Children[0];
                    GenerateExpressionSyntax(argumentExpression, inExpressionStatement);
                }
            }

            string libName, functionName;
            if (isBuiltInFunction)
            {
                libName = Libraries.BuiltInLibName;
                functionName = node.Children[0].Terminator.Text;
            }
            else
            {
                (libName, functionName) = this.GetLibNameAndMemberName(node.Children[0]);
            }

            if (!this.env.Libs.HasFunction(libName, functionName))
            {
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnsupportedInvocationBaseExpression(node.Children[0].Range));
                return;
            }

            int expectedArgumentNumber = isBuiltInFunction ?
                this.env.Libs.GetArgumentNumber(functionName) :
                this.env.Libs.GetArgumentNumber(libName, functionName);
            if (argumentNumber != expectedArgumentNumber)
            {
                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnexpectedArgumentsCount(
                        node.Children[0].Range, argumentNumber, expectedArgumentNumber));
            }
            this.AddInstruction(node.Range, null, Instruction.CALL_LIB, libName, functionName);
        }

        private void GenerateIfStatementSyntax(SyntaxNode node)
        {
            string endLabel = this.NewLabel();

            SyntaxNode ifPartNode = node.Children[0];
            this.GenerateIfConditionalBlock(ifPartNode, endLabel);

            SyntaxNode elseIfPartGroupNode = node.Children[1];
            if (!elseIfPartGroupNode.IsEmpty)
            {
                foreach (var elseIfPartNode in elseIfPartGroupNode.Children)
                {
                    this.GenerateIfConditionalBlock(elseIfPartNode, endLabel);
                }
            }

            SyntaxNode elsePartNode = node.Children[2];
            if (!elsePartNode.IsEmpty)
            {
                SyntaxNode elseBodyNode = elsePartNode.Children[1];
                this.GenerateStatementBlockSyntax(elseBodyNode);
            }

            this.AddInstruction(node.Range, endLabel, Instruction.NOP, null, null);
        }

        private void GenerateIfConditionalBlock(SyntaxNode node, string endLabel)
        {
            SyntaxNode conditionNode = node.Children[1];;
            SyntaxNode bodyNode = node.Children[3];

            string trueLabel = this.NewLabel();
            string falseLabel = this.NewLabel();
            this.GenerateExpressionSyntax(conditionNode, false);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, trueLabel, falseLabel);
            this.AddInstruction(node.Range, trueLabel, Instruction.NOP, null, null);
            this.GenerateStatementBlockSyntax(bodyNode);
            this.AddInstruction(node.Range, null, Instruction.BR, endLabel, null);
            this.AddInstruction(node.Range, falseLabel, Instruction.NOP, null, null);
        }

        private void GenerateWhileStatementSyntax(SyntaxNode node)
        {
            string startLabel = this.NewLabel();
            string bodyLabel = this.NewLabel();
            string endLabel = this.NewLabel();
            this.AddInstruction(node.Range, startLabel, Instruction.NOP, null, null);

            SyntaxNode conditionNode = node.Children[1];
            this.GenerateExpressionSyntax(conditionNode, false);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, bodyLabel, endLabel);
            this.AddInstruction(node.Range, bodyLabel, Instruction.NOP, null, null);

            SyntaxNode bodyNode = node.Children[2];
            this.GenerateStatementBlockSyntax(bodyNode);
            this.AddInstruction(node.Range, null, Instruction.BR, startLabel, null);

            this.AddInstruction(node.Range, endLabel, Instruction.NOP, null, null);
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
            this.AddInstruction(node.Range, null, Instruction.STORE, loopVariableName, null);

            string startLabel = this.NewLabel();
            string endLabel = this.NewLabel();

            // Body statements.
            this.AddInstruction(node.Range, startLabel, Instruction.NOP, null, null);
            if (!bodyNode.IsEmpty)
            {
                this.GenerateStatementBlockSyntax(bodyNode);
            }

            // Calculates the step value and stores it to register[0].
            if (stepExpressionNode != null)
            {
                this.GenerateExpressionSyntax(stepExpressionNode, false);
                this.AddInstruction(node.Range, null, Instruction.SET, "0", null);
            }
            else
            {
                // Default step value is 1.
                this.AddInstruction(node.Range, null, Instruction.PUSH, "1", null);
                this.AddInstruction(node.Range, null, Instruction.SET, "0", null);
            }

            // Adds the loop variable by the step value.
            this.AddInstruction(node.Range, null, Instruction.LOAD, loopVariableName, null);
            this.AddInstruction(node.Range, null, Instruction.GET, "0", null);
            this.AddInstruction(node.Range, null, Instruction.ADD, null, null);
            this.AddInstruction(node.Range, null, Instruction.STORE, loopVariableName, null);

            // If the step value >=0, goto (1), else, goto (2).
            this.AddInstruction(node.Range, null, Instruction.GET, "0", null);
            this.AddInstruction(node.Range, null, Instruction.PUSH, "0", null);
            this.AddInstruction(node.Range, null, Instruction.GE, null, null);
            string leCompareLabel = this.NewLabel();
            string geCompareLabel = this.NewLabel();
            this.AddInstruction(node.Range, null, Instruction.BR_IF, leCompareLabel, geCompareLabel);

            // (1) Checks if the loop variabel is less than or equal to the To value.
            // Continues the for loop if ture, breaks the for loop otherwise.
            this.AddInstruction(node.Range, leCompareLabel, Instruction.NOP, null, null);
            this.AddInstruction(node.Range, null, Instruction.LOAD, loopVariableName, null);
            this.GenerateExpressionSyntax(toExpressionNode, false);
            this.AddInstruction(node.Range, null, Instruction.LE, null, null);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, startLabel, endLabel);

            // (2) Checks if the loop variabel is greater than or equal to the To value.
            // Continues the for loop if ture, breaks the for loop otherwise.
            this.AddInstruction(node.Range, geCompareLabel, Instruction.NOP, null, null);
            this.AddInstruction(node.Range, null, Instruction.LOAD, loopVariableName, null);
            this.GenerateExpressionSyntax(toExpressionNode, false);
            this.AddInstruction(node.Range, null, Instruction.GE, null, null);
            this.AddInstruction(node.Range, null, Instruction.BR_IF, startLabel, endLabel);

            // The end.
            this.AddInstruction(node.Range, endLabel, Instruction.NOP, null, null);
        }
    }
}