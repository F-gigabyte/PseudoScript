using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class RuntimeError : Exception
    {
        public Token Token { get; private set; }

        public RuntimeError(Token token, string message) : base(message)
        {
            this.Token = token;
        }
    }
}
