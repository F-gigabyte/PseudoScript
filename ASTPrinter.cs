using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class ASTPrinter : Expr.Visitor<string>
    {
        public string Print(Expr expr)
        {
            return expr.Accept(this);
        }

        public string VisitAssignExpr(Expr.Assign expr)
        {
            return Parenthesize(expr.name.Lexeme, expr.value);
        }

        public string VisitBinaryExpr(Expr.Binary expr)
        {
            return Parenthesize(expr.oper.Lexeme, expr.left, expr.right);
        }

        public string VisitGroupingExpr(Expr.Grouping expr)
        {
            return Parenthesize("group", expr.expression);
        }

        public string VisitLiteralExpr(Expr.Literal expr)
        {
            if(expr.value == null)
            {
                return Parenthesize("null");
            }
            return Parenthesize(expr.value.ToString());
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Parenthesize(expr.oper.Lexeme, expr.left, expr.right);
        }

        public string VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.oper.Lexeme, expr.right);
        }

        public string VisitVariableExpr(Expr.Variable expr)
        {
            return expr.name.Lexeme;
        }

        private string Parenthesize(string name, params Expr[] expressions)
        {
            string text = $"[";
            foreach(Expr expr in expressions)
            {
                text += $"{expr.Accept(this)} ";
            }
            text += $"{name}]";
            return text;
        }
    }
}
