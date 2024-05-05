using System;
using System.Collections.Generic;

namespace PseudoScript
{    abstract class Stmt
    {
        public interface Visitor<R>
        {
            public R VisitBlockStmt(Block stmt);
            public R VisitExpressionStmt(Expression stmt);
            public R VisitVarStmt(Var stmt);
            public R VisitIfStmt(If stmt);
            public R VisitPrintStmt(Print stmt);
            public R VisitWhileStmt(While stmt);
        }

        public class Block : Stmt
        {
            public Block(LinkedList<Stmt> statements)
            {
                this.statements = statements;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBlockStmt(this);
            }

            public LinkedList<Stmt> statements { get; private set;}
        }

        public class Expression : Stmt
        {
            public Expression(Expr expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }

            public Expr expression { get; private set;}
        }

        public class Var : Stmt
        {
            public Var(Token name)
            {
                this.name = name;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }

            public Token name { get; private set;}
        }

        public class If : Stmt
        {
            public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
            {
                this.condition = condition;
                this.thenBranch = thenBranch;
                this.elseBranch = elseBranch;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitIfStmt(this);
            }

            public Expr condition { get; private set;}
            public Stmt thenBranch { get; private set;}
            public Stmt elseBranch { get; private set;}
        }

        public class Print : Stmt
        {
            public Print(Expr expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }

            public Expr expression { get; private set;}
        }

        public class While : Stmt
        {
            public While(Expr condition, Stmt body)
            {
                this.condition = condition;
                this.body = body;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitWhileStmt(this);
            }

            public Expr condition { get; private set;}
            public Stmt body { get; private set;}
        }


        public abstract R Accept<R>(Visitor<R> visitor);
    }
}
