using System;
using System.Collections.Generic;

namespace PseudoScript
{    abstract class Expr
    {
        public interface Visitor<R>
        {
            public R VisitAssignExpr(Assign expr);
            public R VisitBinaryExpr(Binary expr);
            public R VisitLogicalExpr(Logical expr);
            public R VisitGroupingExpr(Grouping expr);
            public R VisitLiteralExpr(Literal expr);
            public R VisitVariableExpr(Variable expr);
            public R VisitUnaryExpr(Unary expr);
        }

        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }

            public Token name { get; private set;}
            public Expr value { get; private set;}
        }

        public class Binary : Expr
        {
            public Binary(Expr left, Token oper, Expr right)
            {
                this.left = left;
                this.oper = oper;
                this.right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }

            public Expr left { get; private set;}
            public Token oper { get; private set;}
            public Expr right { get; private set;}
        }

        public class Logical : Expr
        {
            public Logical(Expr left, Token oper, Expr right)
            {
                this.left = left;
                this.oper = oper;
                this.right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }

            public Expr left { get; private set;}
            public Token oper { get; private set;}
            public Expr right { get; private set;}
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                this.expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }

            public Expr expression { get; private set;}
        }

        public class Literal : Expr
        {
            public Literal(object value)
            {
                this.value = value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }

            public object value { get; private set;}
        }

        public class Variable : Expr
        {
            public Variable(Token name)
            {
                this.name = name;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitVariableExpr(this);
            }

            public Token name { get; private set;}
        }

        public class Unary : Expr
        {
            public Unary(Token oper, Expr right)
            {
                this.oper = oper;
                this.right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }

            public Token oper { get; private set;}
            public Expr right { get; private set;}
        }


        public abstract R Accept<R>(Visitor<R> visitor);
    }
}
