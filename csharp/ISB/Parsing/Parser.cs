// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Parsing
{
    internal sealed class Parser
    {
        private static readonly TokenKind[] BinaryOperatorPrecedence =
        {
            TokenKind.Or,
            TokenKind.And,
            TokenKind.Equal,
            TokenKind.NotEqual,
            TokenKind.LessThan,
            TokenKind.GreaterThan,
            TokenKind.LessThanOrEqual,
            TokenKind.GreaterThanOrEqual,
            TokenKind.Plus,
            TokenKind.Minus,
            TokenKind.Multiply,
            TokenKind.Divide
        };

        private readonly IReadOnlyList<Token> tokens;
        private readonly DiagnosticBag diagnostics;
        private int index = 0;

        public Parser(IReadOnlyList<Token> tokens, DiagnosticBag diagnostics)
        {
            this.tokens = tokens;
            this.diagnostics = diagnostics;

            var statements = new List<SyntaxNode>();

            while (this.index < this.tokens.Count)
            {
                switch (this.Peek())
                {
                    case TokenKind.Sub:
                        statements.Add(this.ParseSubModuleDeclaration());
                        break;
                    default:
                        statements.Add(this.ParseStatement());
                        break;
                }
            }

            this.SyntaxTree = new SyntaxNode(SyntaxNodeKind.StatementBlockSyntax, statements);
        }

        public SyntaxNode SyntaxTree { get; private set; }

        private TokenKind Peek() => this.tokens[this.index].Kind;

        private Token Eat(TokenKind kind)
        {
            if (this.index < this.tokens.Count)
            {
                Token current = this.tokens[this.index];

                if (current.Kind == kind)
                {
                    this.index++;
                    return current;
                }
                else
                {
                    this.diagnostics.ReportUnexpectedTokenFound(current.Range, current.Kind, kind);
                    return new Token(kind, string.Empty, current.Range);
                }
            }
            else
            {
                var range = this.tokens[this.tokens.Count - 1].Range;
                this.diagnostics.ReportUnexpectedEndOfStream(range, kind);
                return new Token(kind, string.Empty, range);
            }
        }

        private void RunToEndOfLine(bool reportErrors = true)
        {
            var currentLine = this.tokens[this.index - 1].Range.Start.Line;

            while (true)
            {
                if (this.index >= this.tokens.Count)
                {
                    break;
                }

                var currentToken = this.tokens[this.index];

                if (currentToken.Kind == TokenKind.Comment ||
                    currentToken.Kind == TokenKind.Unrecognized ||
                    currentLine < currentToken.Range.Start.Line)
                {
                    break;
                }

                if (reportErrors)
                {
                    this.diagnostics.ReportUnexpectedStatementInsteadOfNewLine(currentToken.Range);
                    reportErrors = false;
                }

                this.index++;
            }
        }

        private SyntaxNode ParseSubModuleDeclaration()
        {
            var subToken = this.Eat(TokenKind.Sub);
            var nameToken = this.Eat(TokenKind.Identifier);
            this.RunToEndOfLine();

            var statements = this.ParseStatementsExcept(TokenKind.Sub, TokenKind.EndSub);

            var endSubToken = this.Eat(TokenKind.EndSub);
            this.RunToEndOfLine();

            return new SyntaxNode(SyntaxNodeKind.SubModuleStatementSyntax,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, subToken),
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, nameToken),
                statements,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, endSubToken)
            );
        }

        private SyntaxNode ParseStatementsExcept(params TokenKind[] kinds)
        {
            var children = new List<SyntaxNode>();
            while (this.index < this.tokens.Count && !kinds.Contains(this.Peek()))
            {
                children.Add(this.ParseStatement());
            }
            return new SyntaxNode(SyntaxNodeKind.StatementBlockSyntax, children);
        }

        private SyntaxNode ParseStatement()
        {
            switch (this.Peek())
            {
                case TokenKind.If:
                    return this.ParseIfStatement();
                case TokenKind.For:
                    return this.ParseForStatement();
                case TokenKind.While:
                    return this.ParseWhileStatement();

                case TokenKind.Identifier:
                    if (this.index + 1 < this.tokens.Count && this.tokens[this.index + 1].Kind == TokenKind.Colon)
                    {
                        var labelToken = this.Eat(TokenKind.Identifier);
                        var colonToken = this.Eat(TokenKind.Colon);
                        this.RunToEndOfLine();

                        return new SyntaxNode(SyntaxNodeKind.LabelStatementSyntax,
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, labelToken),
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, colonToken)
                        );
                    }
                    else
                    {
                        var expression = this.ParseBaseExpression();
                        this.RunToEndOfLine();
                        return expression;
                    }

                case TokenKind.GoTo:
                    var goToToken = this.Eat(TokenKind.GoTo);
                    var identifier = this.Eat(TokenKind.Identifier);
                    this.RunToEndOfLine();

                    return new SyntaxNode(SyntaxNodeKind.GoToStatementSyntax,
                        new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, goToToken),
                        new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, identifier)
                    );

                case TokenKind.Comment:
                    var commentToken = this.Eat(TokenKind.Comment);
                    this.RunToEndOfLine();
                    return new SyntaxNode(SyntaxNodeKind.CommentStatementSyntax, commentToken);

                case TokenKind foundKind:
                    var foundToken = this.Eat(foundKind);
                    this.RunToEndOfLine(reportErrors: false);

                    if (foundKind != TokenKind.Unrecognized)
                    {
                        this.diagnostics.ReportUnexpectedTokenInsteadOfStatement(foundToken.Range, foundToken.Kind);
                    }
                    return new SyntaxNode(SyntaxNodeKind.UnrecognizedStatementSyntax, foundToken);
            }
        }

        private SyntaxNode ParseIfStatement()
        {
            var ifToken = this.Eat(TokenKind.If);
            var expression = this.ParseBaseExpression();
            var thenToken = this.Eat(TokenKind.Then);
            this.RunToEndOfLine();
            var statements = this.ParseStatementsExcept(TokenKind.ElseIf, TokenKind.Else, TokenKind.EndIf);

            var ifPart = new SyntaxNode(SyntaxNodeKind.IfPartSyntax,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, ifToken),
                expression,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, thenToken),
                statements
            );

            var elseIfPartGroupChildren = new List<SyntaxNode>();
            while (this.index < this.tokens.Count && this.Peek() == TokenKind.ElseIf)
            {
                var elseIfToken = this.Eat(TokenKind.ElseIf);
                expression = this.ParseBaseExpression();
                thenToken = this.Eat(TokenKind.Then);
                this.RunToEndOfLine();
                statements = this.ParseStatementsExcept(TokenKind.ElseIf, TokenKind.Else, TokenKind.EndIf);

                var elseIfPart = new SyntaxNode(SyntaxNodeKind.ElseIfPartSyntax,
                    new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, elseIfToken),
                    expression,
                    new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, thenToken),
                    statements
                );
                elseIfPartGroupChildren.Add(elseIfPart);
            }
            SyntaxNode elseIfPartGroup = (elseIfPartGroupChildren.Count > 0) ?
                new SyntaxNode(SyntaxNodeKind.ElseIfPartGroupSyntax, elseIfPartGroupChildren) :
                null;

            SyntaxNode elsePart = null;
            if (this.index < this.tokens.Count && this.Peek() == TokenKind.Else)
            {
                var elseToken = this.Eat(TokenKind.Else);
                this.RunToEndOfLine();
                statements = this.ParseStatementsExcept(TokenKind.ElseIf, TokenKind.Else, TokenKind.EndIf);

                elsePart = new SyntaxNode(SyntaxNodeKind.ElsePartSyntax,
                    new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, elseToken),
                    statements
                );
            }

            var endIfToken = this.Eat(TokenKind.EndIf);
            this.RunToEndOfLine();

            return new SyntaxNode(SyntaxNodeKind.IfStatementSyntax,
                ifPart,
                elseIfPartGroup,
                elsePart,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, endIfToken)
            );
        }

        private SyntaxNode ParseForStatement()
        {
            var forToken = this.Eat(TokenKind.For);
            var identifierToken = this.Eat(TokenKind.Identifier);
            var equalToken = this.Eat(TokenKind.Equal);
            var fromExpression = this.ParseBaseExpression();
            var toToken = this.Eat(TokenKind.To);
            var toExpression = this.ParseBaseExpression();

            SyntaxNode stepClause = null;
            if (this.index < this.tokens.Count && this.Peek() == TokenKind.Step)
            {
                var stepToken = this.Eat(TokenKind.Step);
                var expression = this.ParseBaseExpression();

                stepClause = new SyntaxNode(SyntaxNodeKind.ForStepClauseSyntax,
                    new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, stepToken),
                    expression
                );
            }

            this.RunToEndOfLine();
            var statements = this.ParseStatementsExcept(TokenKind.EndFor);
            var endForToken = this.Eat(TokenKind.EndFor);
            this.RunToEndOfLine();

            return new SyntaxNode(SyntaxNodeKind.ForStatementSyntax,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, forToken),
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, identifierToken),
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, equalToken),
                fromExpression,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, toToken),
                toExpression,
                stepClause,
                statements,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, endForToken)
            );
        }

        private SyntaxNode ParseWhileStatement()
        {
            var whileToken = this.Eat(TokenKind.While);
            var expression = this.ParseBaseExpression();
            this.RunToEndOfLine();

            var statements = this.ParseStatementsExcept(TokenKind.EndWhile);

            var endWhileToken = this.Eat(TokenKind.EndWhile);
            this.RunToEndOfLine();

            return new SyntaxNode(SyntaxNodeKind.WhileStatementSyntax,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, whileToken),
                expression,
                statements,
                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, endWhileToken)
            );
        }

        private SyntaxNode ParseBaseExpression()
        {
            return this.ParseBinaryOperator(precedence: 0);
        }

        private SyntaxNode ParseBinaryOperator(int precedence)
        {
            if (precedence >= BinaryOperatorPrecedence.Length)
            {
                return this.ParseUnaryOperator();
            }

            var expression = this.ParseBinaryOperator(precedence + 1);
            var expectedOperatorKind = BinaryOperatorPrecedence[precedence];

            while (this.index < this.tokens.Count && this.Peek() == expectedOperatorKind)
            {
                var operatorToken = this.Eat(expectedOperatorKind);
                var rightHandSide = this.ParseBinaryOperator(precedence + 1);

                expression = new SyntaxNode(SyntaxNodeKind.BinaryOperatorExpressionSyntax,
                    expression,
                    new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, operatorToken),
                    rightHandSide
                );
            }

            return expression;
        }

        private SyntaxNode ParseUnaryOperator()
        {
            if (this.index < this.tokens.Count && this.Peek() == TokenKind.Minus)
            {
                var minusToken = this.Eat(TokenKind.Minus);
                var expression = this.ParseCoreExpression();

                return new SyntaxNode(SyntaxNodeKind.UnaryOperatorExpressionSyntax,
                    new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, minusToken),
                    expression
                );
            }

            return this.ParseCoreExpression();
        }

        private SyntaxNode ParseCoreExpression()
        {
            var expression = this.ParseTerminalExpression();

            while (this.index < this.tokens.Count)
            {
                switch (this.Peek())
                {
                    case TokenKind.Dot:
                        var dotToken = this.Eat(TokenKind.Dot);
                        var identifierToken = this.Eat(TokenKind.Identifier);
                        expression = new SyntaxNode(SyntaxNodeKind.ObjectAccessExpressionSyntax,
                            expression,
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, dotToken),
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, identifierToken)
                        );
                        break;

                    case TokenKind.LeftBracket:
                        var leftBracketToken = this.Eat(TokenKind.LeftBracket);
                        var indexExpression = this.ParseBaseExpression();
                        var rightBracketToken = this.Eat(TokenKind.RightBracket);
                        expression = new SyntaxNode(SyntaxNodeKind.ArrayAccessExpressionSyntax,
                            expression,
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, leftBracketToken),
                            indexExpression,
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, rightBracketToken)
                        );
                        break;

                    case TokenKind.LeftParen:
                        var leftParenToken = this.Eat(TokenKind.LeftParen);
                        var arguments = this.ParseArguments();
                        var rightParenToken = this.Eat(TokenKind.RightParen);
                        expression = new SyntaxNode(SyntaxNodeKind.InvocationExpressionSyntax,
                            expression,
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, leftParenToken),
                            arguments,
                            new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, rightParenToken)
                        );
                        break;

                    default:
                        return expression;
                }
            }
            return expression;
        }

        private SyntaxNode ParseArguments()
        {
            var arguments = new List<SyntaxNode>();
            SyntaxNode currentArgument = null;

            while (this.index < this.tokens.Count)
            {
                if (currentArgument != null)
                {
                    switch (this.Peek())
                    {
                        case TokenKind.Comma:
                            var commaTokenOpt = this.Eat(TokenKind.Comma);
                            arguments.Add(new SyntaxNode(SyntaxNodeKind.ArgumentSyntax,
                                currentArgument,
                                new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, commaTokenOpt)
                            ));
                            currentArgument = null;
                            break;

                        case TokenKind.RightParen:
                            arguments.Add(new SyntaxNode(SyntaxNodeKind.ArgumentSyntax,
                                currentArgument,
                                null
                            ));
                            return new SyntaxNode(SyntaxNodeKind.ArgumentGroupSyntax, arguments);

                        case TokenKind foundKind:
                            this.diagnostics.ReportUnexpectedTokenFound(this.tokens[this.index].Range, foundKind, TokenKind.Comma);
                            arguments.Add(new SyntaxNode(SyntaxNodeKind.ArgumentSyntax,
                                currentArgument,
                                null
                            ));
                            currentArgument = null;
                            break;
                    }
                }
                else if (this.Peek() == TokenKind.RightParen)
                {
                    return new SyntaxNode(SyntaxNodeKind.ArgumentGroupSyntax, arguments);
                }
                else
                {
                    currentArgument = this.ParseBaseExpression();
                }
            }

            if (currentArgument != null)
            {
                arguments.Add(new SyntaxNode(SyntaxNodeKind.ArgumentSyntax,
                    currentArgument,
                    null
                ));
            }

            return new SyntaxNode(SyntaxNodeKind.ArgumentGroupSyntax, arguments);
        }

        private SyntaxNode ParseTerminalExpression()
        {
            if (this.index >= this.tokens.Count)
            {
                var range = this.tokens[this.tokens.Count - 1].Range;
                var missingToken = new Token(TokenKind.Identifier, string.Empty, range);

                this.diagnostics.ReportUnexpectedEndOfStream(range, missingToken.Kind);
                return new SyntaxNode(SyntaxNodeKind.IdentifierExpressionSyntax, missingToken);
            }

            switch (this.Peek())
            {
                case TokenKind.Identifier:
                    var identifierToken = this.Eat(TokenKind.Identifier);
                    return new SyntaxNode(SyntaxNodeKind.IdentifierExpressionSyntax, identifierToken);

                case TokenKind.NumberLiteral:
                    var numberToken = this.Eat(TokenKind.NumberLiteral);
                    return new SyntaxNode(SyntaxNodeKind.NumberLiteralExpressionSyntax, numberToken);

                case TokenKind.StringLiteral:
                    var stringToken = this.Eat(TokenKind.StringLiteral);
                    return new SyntaxNode(SyntaxNodeKind.StringLiteralExpressionSyntax, stringToken);

                case TokenKind.LeftParen:
                    var leftParenToken = this.Eat(TokenKind.LeftParen);
                    var expression = this.ParseBaseExpression();
                    var rightParenToken = this.Eat(TokenKind.RightParen);
                    return new SyntaxNode(SyntaxNodeKind.ParenthesisExpressionSyntax,
                        new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, leftParenToken),
                        expression,
                        new SyntaxNode(SyntaxNodeKind.TerminatorSyntax, rightParenToken)
                    );

                case TokenKind foundKind:
                    var foundToken = this.Eat(foundKind);

                    if (foundKind != TokenKind.Unrecognized)
                    {
                        this.diagnostics.ReportUnexpectedTokenFound(foundToken.Range, foundKind, TokenKind.Identifier);
                    }

                    return new SyntaxNode(SyntaxNodeKind.UnrecognizedExpressionSyntax, foundToken);
            }
        }
    }
}