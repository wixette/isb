// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using ISB.Scanning;
using ISB.Utilities;

namespace ISB.Parsing
{
    // A stateless class that does not hold any info between two passes.
    public sealed class Parser
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

        private Parser(IReadOnlyList<Token> tokens, DiagnosticBag diagnostics)
        {
            this.tokens = tokens;
            this.diagnostics = diagnostics;
        }

        public static SyntaxNode Parse(IReadOnlyList<Token> tokens, DiagnosticBag diagnostics)
        {
            Parser parser = new Parser(tokens, diagnostics);
            return parser.ParseInternal();
        }

        private SyntaxNode ParseInternal()
        {
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

            if (statements.Count > 0)
            {
                return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.StatementBlockSyntax, statements);
            }
            else
            {
                return SyntaxNode.CreateEmpty();
            }
        }

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
                    if (this.diagnostics != null)
                        this.diagnostics.Add(
                                Diagnostic.ReportUnexpectedTokenFound(current.Range, current.Kind, kind));
                    return new Token(kind, string.Empty, current.Range);
                }
            }
            else
            {
                var range = this.tokens[this.tokens.Count - 1].Range;
                if (this.diagnostics != null)
                    this.diagnostics.Add(
                        Diagnostic.ReportUnexpectedEndOfStream(range, kind));
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
                    if (this.diagnostics != null)
                        this.diagnostics.Add(Diagnostic.ReportUnexpectedStatementInsteadOfNewLine(currentToken.Range));
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

            return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.SubModuleStatementSyntax,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, subToken),
                SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, nameToken),
                statements,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, endSubToken)
            );
        }

        private SyntaxNode ParseStatementsExcept(params TokenKind[] kinds)
        {
            var children = new List<SyntaxNode>();
            while (this.index < this.tokens.Count && !kinds.Contains(this.Peek()))
            {
                children.Add(this.ParseStatement());
            }
            return children.Count > 0 ?
                SyntaxNode.CreateNonTerminal(SyntaxNodeKind.StatementBlockSyntax, children) :
                SyntaxNode.CreateEmpty();
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

                        return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.LabelStatementSyntax,
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, labelToken),
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, colonToken)
                        );
                    }
                    else
                    {
                        return this.ParseExpressionStatement();
                    }

                // The original MSB Parser does not accept standalone expression
                // statements such as "3.14", "(1+1)*2", etc. The error message
                // looks like:
                //
                // "I didn't expect to see 'string' here."
                // "I was expecting the start of a new statement."
                //
                // An interactive scripting language environemnt need support
                // standalone expression statements to make itself a quick
                // "calculator".
                case TokenKind.Minus:
                case TokenKind.NumberLiteral:
                case TokenKind.StringLiteral:
                case TokenKind.LeftParen:
                    return this.ParseExpressionStatement();

                case TokenKind.GoTo:
                    var goToToken = this.Eat(TokenKind.GoTo);
                    var identifier = this.Eat(TokenKind.Identifier);
                    this.RunToEndOfLine();

                    return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.GoToStatementSyntax,
                        SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, goToToken),
                        SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, identifier)
                    );

                case TokenKind.Comment:
                    var commentToken = this.Eat(TokenKind.Comment);
                    this.RunToEndOfLine();
                    return SyntaxNode.CreateTerminal(SyntaxNodeKind.CommentStatementSyntax, commentToken);

                case TokenKind foundKind:
                    var foundToken = this.Eat(foundKind);
                    this.RunToEndOfLine(reportErrors: false);

                    if (foundKind != TokenKind.Unrecognized)
                    {
                        if (this.diagnostics != null)
                            this.diagnostics.Add(
                                Diagnostic.ReportUnexpectedTokenInsteadOfStatement(foundToken.Range, foundToken.Kind));
                    }
                    return SyntaxNode.CreateTerminal(SyntaxNodeKind.UnrecognizedStatementSyntax, foundToken);
            }
        }

        private SyntaxNode ParseIfStatement()
        {
            var ifToken = this.Eat(TokenKind.If);
            var expression = this.ParseExpression();
            var thenToken = this.Eat(TokenKind.Then);
            this.RunToEndOfLine();
            var statements = this.ParseStatementsExcept(TokenKind.ElseIf, TokenKind.Else, TokenKind.EndIf);

            var ifPart = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.IfPartSyntax,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, ifToken),
                expression,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, thenToken),
                statements
            );

            var elseIfPartGroupChildren = new List<SyntaxNode>();
            while (this.index < this.tokens.Count && this.Peek() == TokenKind.ElseIf)
            {
                var elseIfToken = this.Eat(TokenKind.ElseIf);
                expression = this.ParseExpression();
                thenToken = this.Eat(TokenKind.Then);
                this.RunToEndOfLine();
                statements = this.ParseStatementsExcept(TokenKind.ElseIf, TokenKind.Else, TokenKind.EndIf);

                var elseIfPart = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ElseIfPartSyntax,
                    SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, elseIfToken),
                    expression,
                    SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, thenToken),
                    statements
                );
                elseIfPartGroupChildren.Add(elseIfPart);
            }
            SyntaxNode elseIfPartGroup = (elseIfPartGroupChildren.Count > 0) ?
                SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ElseIfPartGroupSyntax, elseIfPartGroupChildren) :
                SyntaxNode.CreateEmpty();

            SyntaxNode elsePart = null;
            if (this.index < this.tokens.Count && this.Peek() == TokenKind.Else)
            {
                var elseToken = this.Eat(TokenKind.Else);
                this.RunToEndOfLine();
                statements = this.ParseStatementsExcept(TokenKind.ElseIf, TokenKind.Else, TokenKind.EndIf);

                elsePart = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ElsePartSyntax,
                    SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, elseToken),
                    statements
                );
            }
            else
            {
                elsePart = SyntaxNode.CreateEmpty();
            }

            var endIfToken = this.Eat(TokenKind.EndIf);
            this.RunToEndOfLine();

            return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.IfStatementSyntax,
                ifPart,
                elseIfPartGroup,
                elsePart,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, endIfToken)
            );
        }

        private SyntaxNode ParseForStatement()
        {
            var forToken = this.Eat(TokenKind.For);
            var identifierToken = this.Eat(TokenKind.Identifier);
            var equalToken = this.Eat(TokenKind.Equal);
            var fromExpression = this.ParseExpression();
            var toToken = this.Eat(TokenKind.To);
            var toExpression = this.ParseExpression();

            SyntaxNode stepClause = null;
            if (this.index < this.tokens.Count && this.Peek() == TokenKind.Step)
            {
                var stepToken = this.Eat(TokenKind.Step);
                var expression = this.ParseExpression();

                stepClause = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ForStepClauseSyntax,
                    SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, stepToken),
                    expression
                );
            }
            else
            {
                stepClause = SyntaxNode.CreateEmpty();
            }

            this.RunToEndOfLine();
            var statements = this.ParseStatementsExcept(TokenKind.EndFor);
            var endForToken = this.Eat(TokenKind.EndFor);
            this.RunToEndOfLine();

            return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ForStatementSyntax,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, forToken),
                SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, identifierToken),
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, equalToken),
                fromExpression,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, toToken),
                toExpression,
                stepClause,
                statements,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, endForToken)
            );
        }

        private SyntaxNode ParseWhileStatement()
        {
            var whileToken = this.Eat(TokenKind.While);
            var expression = this.ParseExpression();
            this.RunToEndOfLine();

            var statements = this.ParseStatementsExcept(TokenKind.EndWhile);

            var endWhileToken = this.Eat(TokenKind.EndWhile);
            this.RunToEndOfLine();

            return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.WhileStatementSyntax,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, whileToken),
                expression,
                statements,
                SyntaxNode.CreateTerminal(SyntaxNodeKind.KeywordSyntax, endWhileToken)
            );
        }

        private SyntaxNode ParseExpressionStatement()
        {
            var expression = this.ParseExpression();
            this.RunToEndOfLine();
            return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ExpressionStatementSyntax, expression);
        }

        private SyntaxNode ParseExpression()
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

                expression = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.BinaryOperatorExpressionSyntax,
                    expression,
                    SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, operatorToken),
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

                return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.UnaryOperatorExpressionSyntax,
                    SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, minusToken),
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
                        expression = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ObjectAccessExpressionSyntax,
                            expression,
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, dotToken),
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, identifierToken)
                        );
                        break;

                    case TokenKind.LeftBracket:
                        var leftBracketToken = this.Eat(TokenKind.LeftBracket);
                        var indexExpression = this.ParseExpression();
                        var rightBracketToken = this.Eat(TokenKind.RightBracket);
                        expression = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArrayAccessExpressionSyntax,
                            expression,
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, leftBracketToken),
                            indexExpression,
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, rightBracketToken)
                        );
                        break;

                    case TokenKind.LeftParen:
                        var leftParenToken = this.Eat(TokenKind.LeftParen);
                        var arguments = this.ParseArguments();
                        var rightParenToken = this.Eat(TokenKind.RightParen);
                        expression = SyntaxNode.CreateNonTerminal(SyntaxNodeKind.InvocationExpressionSyntax,
                            expression,
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, leftParenToken),
                            arguments,
                            SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, rightParenToken)
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
                            var commaToken = this.Eat(TokenKind.Comma);
                            arguments.Add(SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentSyntax,
                                currentArgument,
                                SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, commaToken)
                            ));
                            currentArgument = null;
                            break;

                        case TokenKind.RightParen:
                            arguments.Add(SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentSyntax,
                                currentArgument,
                                SyntaxNode.CreateEmpty()
                            ));
                            return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentGroupSyntax, arguments);

                        case TokenKind foundKind:
                            if (this.diagnostics != null)
                                this.diagnostics.Add(
                                        Diagnostic.ReportUnexpectedTokenFound(
                                            this.tokens[this.index].Range, foundKind, TokenKind.Comma));
                            arguments.Add(SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentSyntax,
                                currentArgument,
                                SyntaxNode.CreateEmpty()
                            ));
                            currentArgument = null;
                            break;
                    }
                }
                else if (this.Peek() == TokenKind.RightParen)
                {
                    return arguments.Count > 0 ?
                        SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentGroupSyntax, arguments) :
                        SyntaxNode.CreateEmpty();
                }
                else
                {
                    currentArgument = this.ParseExpression();
                }
            }

            if (currentArgument != null)
            {
                arguments.Add(SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentSyntax,
                    currentArgument,
                    SyntaxNode.CreateEmpty()
                ));
            }

            return arguments.Count > 0 ?
                SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ArgumentGroupSyntax, arguments) :
                SyntaxNode.CreateEmpty();
        }

        private SyntaxNode ParseTerminalExpression()
        {
            if (this.index >= this.tokens.Count)
            {
                var range = this.tokens[this.tokens.Count - 1].Range;
                var missingToken = new Token(TokenKind.Identifier, string.Empty, range);

                if (this.diagnostics != null)
                    this.diagnostics.Add(Diagnostic.ReportUnexpectedEndOfStream(range, missingToken.Kind));
                return SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, missingToken);
            }

            switch (this.Peek())
            {
                case TokenKind.Identifier:
                    var identifierToken = this.Eat(TokenKind.Identifier);
                    return SyntaxNode.CreateTerminal(SyntaxNodeKind.IdentifierExpressionSyntax, identifierToken);

                case TokenKind.NumberLiteral:
                    var numberToken = this.Eat(TokenKind.NumberLiteral);
                    return SyntaxNode.CreateTerminal(SyntaxNodeKind.NumberLiteralExpressionSyntax, numberToken);

                case TokenKind.StringLiteral:
                    var stringToken = this.Eat(TokenKind.StringLiteral);
                    return SyntaxNode.CreateTerminal(SyntaxNodeKind.StringLiteralExpressionSyntax, stringToken);

                case TokenKind.LeftParen:
                    var leftParenToken = this.Eat(TokenKind.LeftParen);
                    var expression = this.ParseExpression();
                    var rightParenToken = this.Eat(TokenKind.RightParen);
                    return SyntaxNode.CreateNonTerminal(SyntaxNodeKind.ParenthesisExpressionSyntax,
                        SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, leftParenToken),
                        expression,
                        SyntaxNode.CreateTerminal(SyntaxNodeKind.PunctuationSyntax, rightParenToken)
                    );

                case TokenKind foundKind:
                    var foundToken = this.Eat(foundKind);

                    if (foundKind != TokenKind.Unrecognized)
                    {
                        if (this.diagnostics != null)
                            this.diagnostics.Add(Diagnostic.ReportUnexpectedTokenFound(
                                foundToken.Range, foundKind, TokenKind.Identifier));
                    }

                    return SyntaxNode.CreateTerminal(SyntaxNodeKind.UnrecognizedExpressionSyntax, foundToken);
            }
        }
    }
}