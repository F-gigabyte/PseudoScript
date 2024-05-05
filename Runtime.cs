using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class Runtime
    {
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();
        private Runtime enclosing;

        public Runtime()
        {
            enclosing = null;
        }

        public Runtime(Runtime enclosing)
        {
            this.enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            values.Add(name, value);
        }

        public void Assign(Token name, object value)
        {
            if(values.ContainsKey(name.Lexeme))
            {
                values[name.Lexeme] = value;
            }
            else if(Defined(name.Lexeme))
            {
                enclosing.Assign(name, value);
            }
            else
            {
                Define(name.Lexeme, value);
            }
        }

        public bool Defined(string name)
        {
            if(values.ContainsKey(name))
            {
                return true;
            }
            if(enclosing != null)
            {
                return enclosing.Defined(name);
            }
            return false;
        }

        public object Get(Token name)
        {
            if(values.ContainsKey(name.Lexeme))
            {
                return values[name.Lexeme];
            }
            if(enclosing != null)
            {
                return enclosing.Get(name);
            }
            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }
    }
}
