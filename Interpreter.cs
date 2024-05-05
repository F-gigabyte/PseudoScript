using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class Interpreter : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private Runtime env = new Runtime();

        public object Interpret(LinkedList<Stmt> program)
        {
            object value = null;
            try
            {
                foreach(Stmt statement in program)
                {
                    value = Execute(statement);
                }
                return value;
            }
            catch(RuntimeError e)
            {
                Program.RuntimeError(e);
                return value;
            }
        }

        private object Execute(Stmt line)
        {
            return line.Accept(this);
        }

        public string Stringify(object obj)
        {
            if(obj == null)
            {
                return "?";
            }
            if(IsOfType(obj, "Double"))
            {
                double value = (double)obj;
                if(Double.IsPositiveInfinity(value))
                {
                    return "Infinity";
                }
                else if(Double.IsNegativeInfinity(value))
                {
                    return "-Infinity";
                }
                else
                {
                    return value.ToString();
                }
            }
            if(IsOfType(obj, "Int64"))
            {
                return obj.ToString();
            }
            if(IsOfType(obj, "Boolean"))
            {
                bool val = (bool)obj;
                if(val)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            return obj.ToString();
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);
            switch(expr.oper.Type)
            {
                case (TokenType.Minus):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    return (dynamic)left - (dynamic)right;
                }
                case (TokenType.Plus):
                {
                    if(IsOfType(left, "String") && right != null)
                    {
                        return left.ToString() + Stringify(right);
                    }
                    else if(IsOfType(right, "String") && left != null)
                    {
                        return Stringify(left) + right.ToString();
                    }
                    else if(IsNumber(left) && IsNumber(right))
                    {
                        return (dynamic)left + (dynamic)right;
                    }
                    throw new RuntimeError(expr.oper, "Operands must be two numbers or a string and non null.");
                }
                case (TokenType.Slash):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    if(IsOfType(left, "Int64") && IsOfType(right, "Int64") && (Int64)right == 0)
                    {
                        throw new RuntimeError(expr.oper, "Can't divide integer by 0.");
                    }
                    return (dynamic)left / (dynamic)right;
                }
                case (TokenType.Star):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    return (dynamic)left * (dynamic)right;
                }
                case (TokenType.Greater):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    return (dynamic)left > (dynamic)right;
                }
                case (TokenType.Less):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    return (dynamic)left < (dynamic)right;
                }
                case (TokenType.GreaterEqual):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    return (dynamic)left >= (dynamic)right;
                }
                case (TokenType.LessEqual):
                {
                    CheckNumberOperands(expr.oper, left, right);
                    return (dynamic)left <= (dynamic)right;
                }
                case (TokenType.Equal):
                {
                    return IsEqual(left, right);
                }
                case (TokenType.Unequal):
                {
                    return !IsEqual(left, right);
                }
            }
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);
            switch(expr.oper.Type)
            {
                case (TokenType.Minus):
                {
                    CheckNumberOperand(expr.oper, right);
                    return -(dynamic)right;
                }
                case (TokenType.Not):
                {
                    return !IsTruthy(right);
                }
            }
            return null;
        }

        private bool IsTruthy(object obj)
        {
            if(obj == null)
            {
                return false;
            }
            string typeName = obj.GetType().Name;
            if(typeName == "Boolean")
            {
                return (bool)obj;
            }
            else if(typeName == "String")
            {
                return ((string)obj).Length == 0;
            }
            else if(typeName == "Int64")
            {
                return ((Int64)obj) != 0;
            }
            else if(typeName == "Double")
            {
                return ((double)obj) != 0;
            }
            return true;
        }

        private bool IsOfType(object obj, string type)
        {
            if(obj == null)
            {
                return false;
            }
            return obj.GetType().Name == type;
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private bool IsEqual(object a, object b)
        {
            if(a == null && b == null)
            {
                return true;
            }
            else if(a == null)
            {
                if(IsOfType(b, "Boolean"))
                {
                    return b.Equals(IsTruthy(a));
                }
                return false;
            }
            if(IsOfType(a, "Boolean"))
            {
                return a.Equals(IsTruthy(b));
            }
            else if(IsOfType(b, "Boolean"))
            {
                return b.Equals(IsTruthy(a));
            }
            return a.Equals(b);
        }

        private bool IsNumber(object value)
        {
            if(value == null)
            {
                return false;
            }
            string valueType = value.GetType().Name;
            return valueType == "Int64" || valueType == "Double";
        }

        private void CheckNumberOperand(Token oper, object operand)
        {
            if(IsNumber(operand))
            {
                return;
            }
            throw new RuntimeError(oper, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token oper, object left, object right)
        {
            if(IsNumber(left) && IsNumber(right))
            {
                return;
            }
            throw new RuntimeError(oper, "Operands must be numbers.");
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            return Evaluate(stmt.expression);
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            env.Define(stmt.name.Lexeme, null);
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return env.Get(expr.name);
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            env.Assign(expr.name, value);
            return value;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Runtime(env));
            return null;
        }

        void ExecuteBlock(LinkedList<Stmt> statements, Runtime env)
        {
            Runtime prev = this.env;
            try
            {
                this.env = env;
                foreach(Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                this.env = prev;
            }
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if(IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else
            {
                Execute(stmt.elseBranch);
            }
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);
            if(expr.oper.Type == TokenType.And)
            {
                if(!IsTruthy(left))
                {
                    return left;
                }
            }
            else if(expr.oper.Type == TokenType.Or)
            {
                if(IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                throw new NotImplementedException("VisitLogicalExpr");
            }
            return Evaluate(expr.right);
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while(IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }
            return null;
        }
    }
}
