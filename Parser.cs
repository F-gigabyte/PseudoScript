using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class Parser
    {
        private LinkedList<Token> tokens;
        private LinkedListNode<Token> current;

        private class ParseError : Exception
        {

        }

        public Parser(LinkedList<Token> tokens)
        {
            this.tokens = tokens;
            current = tokens.First;
        }

        public LinkedList<Stmt> Parse()
        {
            LinkedList<Stmt> statements = new LinkedList<Stmt>();
            while(!AtEnd())
            {
                statements.AddLast(Declaration());
            }
            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if(Match(TokenType.Identifier))
                {
                    return VarDeclaration();
                }
                return Statement();
            }
            catch(ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt VarDeclaration()
        {
            Token name = Previous();
            if(Match(TokenType.NewLine))
            {
                return new Stmt.Var(name);
            }
            current = current.Previous;
            return Statement();
        }

        private Stmt Statement()
        {
            if(Match(TokenType.If))
            {
                return IfStatement();
            }
            else if(Match(TokenType.Print))
            {
                return PrintStatement();
            }
            else if(Match(TokenType.For))
            {
                return ForStatement();
            }
            else if(Match(TokenType.While))
            {
                return WhileStatement();
            }
            else if(Match(TokenType.Begin))
            {
                Consume(TokenType.NewLine, "Expect newline after begin.");
                return new Stmt.Block(Block());
            }
            return ExpressionStatement();
        }

        private Stmt ForStatement()
        {
            Expr initialiser;
            Token current = Peek();
            if(current.Type != TokenType.Identifier)
            {
                Error(current, "Expect for loop range iterator.");
            }
            initialiser = Assignment();
            Expr condition;
            Stmt body;
            Expr increment;
            if(Match(TokenType.Comma)) // user defined increment
            {
                condition = Expression();
                Consume(TokenType.NewLine, "Expect new line after for statement");
                body = Statement();
                Consume(TokenType.Set, "Expect 'set' after user defined incrementing for loop");
                increment = Expression();
                Consume(TokenType.NewLine, "Expect new line after set expression");
            }
            else
            {
                Consume(TokenType.To, "Expect 'to' after non user defined for loop assignment.");
                Expr end = Unary();
                condition = new Expr.Binary(new Expr.Variable(current), new Token(TokenType.LessEqual, "less equal", null, -1, -1), end);
                Consume(TokenType.NewLine, "Expect new line after for statement");
                body = Statement();
                Consume(TokenType.End, "Expect 'end' after auto incrementing for loop");
                Consume(TokenType.NewLine, "Expect new line after end");
                increment = new Expr.Assign(current, new Expr.Binary(new Expr.Variable(current), new Token(TokenType.Plus, "+", null, -1, -1), new Expr.Literal((object)1L)));
            }
            body = new Stmt.Block(new LinkedList<Stmt>(new Stmt[]{ body, new Stmt.Expression(increment)}));
            body = new Stmt.While(condition, body);
            body = new Stmt.Block(new LinkedList<Stmt>(new Stmt[] { new Stmt.Expression(initialiser), body }));
            return body;
        }

        private Stmt WhileStatement()
        {
            Expr condition = Expression();
            Consume(TokenType.Do, "Expect 'do' after while condition.");
            Consume(TokenType.NewLine, "Expect newline after 'do'.");
            Stmt body = new Stmt.Block(Block());
            return new Stmt.While(condition, body);
        }

        private Stmt IfStatement()
        {
            Expr condition = Expression();
            Consume(TokenType.Then, "Expect 'then' after if or elif condition.");
            Consume(TokenType.NewLine, "Expect newline after 'then'.");
            LinkedList<Stmt> statements = new LinkedList<Stmt>();
            while(!AtEnd() && !Check(TokenType.End) && !Check(TokenType.Elif) && !Check(TokenType.Else))
            {
                statements.AddLast(Declaration());
            }
            if(AtEnd())
            {
                throw Error(Previous(), "None terminated if statement.");
            }
            Stmt thenBranch = new Stmt.Block(statements);
            Stmt elseBranch = null;
            if(Match(TokenType.Else))
            {
                Consume(TokenType.Then, "Expect 'then' after else condition.");
                Consume(TokenType.NewLine, "Expect newline after 'then'.");
                elseBranch = new Stmt.Block(Block());
            }
            else if(Match(TokenType.Elif))
            {
                elseBranch = IfStatement();
            }
            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        private LinkedList<Stmt> Block()
        {
            LinkedList<Stmt> statements = new LinkedList<Stmt>();
            while(!Check(TokenType.End) && !AtEnd())
            {
                statements.AddLast(Declaration());
            }
            Consume(TokenType.End, "Expect 'end' after block.");
            Consume(TokenType.NewLine, "Expect newline after end.");
            return statements;
        }

        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.NewLine, "Expect newline after value.");
            return new Stmt.Print(value);
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.NewLine, "Expect newline after expression.");
            return new Stmt.Expression(expr);
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();
            if(Match(TokenType.LessDash))
            {
                Token oper = Previous();
                Expr value = Assignment();
                string exprType = expr.GetType().Name;
                if(exprType == "Variable")
                {
                    Token name = ((Expr.Variable)expr).name;
                    return new Expr.Assign(name, value);
                }
                Error(oper, "Invalid assignment target.");
            }
            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();
            while(Match(TokenType.Or))
            {
                Token oper = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, oper, right);
            }
            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();
            while(Match(TokenType.And))
            {
                Token oper = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, oper, right);
            }
            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();
            while(Match(TokenType.Unequal, TokenType.Equal))
            {
                Token oper = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Term();

            while(Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                Token oper = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }

        private Expr Term()
        {
            Expr expr = Factor();
            while(Match(TokenType.Minus, TokenType.Plus))
            {
                Token oper = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }

        private Expr Factor()
        {
            Expr expr = Unary();
            while(Match(TokenType.Slash, TokenType.Star))
            {
                Token oper = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, oper, right);
            }
            return expr;
        }

        private Expr Unary()
        {
            if(Match(TokenType.Minus, TokenType.Not))
            {
                Token oper = Previous();
                Expr right = Unary();
                return new Expr.Unary(oper, right);
            }
            else
            {
                return Primary();
            }
        }

        private Expr Primary()
        {
            if(Match(TokenType.False))
            {
                return new Expr.Literal(false);
            }
            else if(Match(TokenType.True))
            {
                return new Expr.Literal(true);
            }
            else if(Match(TokenType.Null))
            {
                return new Expr.Literal(null);
            }
            else if(Match(TokenType.Integer, TokenType.Float, TokenType.String))
            {
                Token lit = Previous();
                return new Expr.Literal(lit.Literal);
            }
            else if(Match(TokenType.Identifier))
            {
                return new Expr.Variable(Previous());
            }
            else if(Match(TokenType.LeftPar))
            {
                Expr expr = Expression();
                Consume(TokenType.RightPar, "Expect ')' after expression.");
                return new Expr.Grouping(expr);
            }
            FalseEquality();
            throw Error(Peek(), "Expect expression.");
        }

        private void FalseEquality()
        {
            FalseComparison();
            if(Match(TokenType.Equal, TokenType.Unequal))
            {
                Token t = Previous();
                Comparison();
                throw Error(t, "Expected left side of equality.");
            }
        }

        private void FalseComparison()
        {
            FalseTerm();
            if(Match(TokenType.Greater, TokenType.Less, TokenType.GreaterEqual, TokenType.LessEqual))
            {
                Token t = Previous();
                Term();
                throw Error(t, "Expected left side of comparison.");
            }
        }

        private void FalseTerm()
        {
            FalseFactor();
            if(Match(TokenType.Minus, TokenType.Plus))
            {
                Token t = Previous();
                Factor();
                throw Error(t, "Expected left side of term.");
            }
        }

        private void FalseFactor()
        {
            if(Match(TokenType.Star, TokenType.Slash))
            {
                Token t = Previous();
                Unary();
                throw Error(t, "Expected left side of factor.");
            }
        }

        private bool Match(params TokenType[] types)
        {
            foreach(TokenType type in types)
            {
                if(Check(type))
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if(Check(type))
            {
                return Advance();
            }
            throw Error(Peek(), message);
        }

        private bool Check(TokenType type)
        {
            if(AtEnd())
            {
                return false;
            }
            return Peek().Type == type;
        }

        private Token Peek()
        {
            return current.Value;
        }

        private bool AtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Previous()
        {
            return current.Previous.Value;
        }

        private Token Advance()
        {
            if(!AtEnd())
            {
                current = current.Next;
            }
            return Previous();
        }

        private ParseError Error(Token token, string message)
        {
            Program.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();
            while(!AtEnd())
            {
                if(Previous().Type == TokenType.NewLine)
                {
                    return;
                }
                switch(Peek().Type)
                {
                    case (TokenType.Class):
                    case (TokenType.For):
                    case (TokenType.Func):
                    case (TokenType.If):
                    case (TokenType.Print):
                    case (TokenType.Input):
                    case (TokenType.Ret):
                    case (TokenType.Identifier):
                    case (TokenType.While):
                    {
                        return;
                    }
                }
                Advance();
            }
        }
    }
}
