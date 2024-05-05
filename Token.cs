using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class Token
    {
        public TokenType Type { get; private set; }
        public string Lexeme { get; private set; }
        public object Literal { get; private set; }
        public int Line { get; private set; }
        public int Column { get; private set; }

        public Token(TokenType type, String lexeme, object literal, int line, int column)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return "{[" + Type + "]: " + Lexeme + ": " + Literal + "}";
        }
    }
}
