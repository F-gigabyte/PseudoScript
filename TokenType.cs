using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    enum TokenType
    {
        LeftSquare, RightSquare, LeftPar, RightPar, Comma, Dot, Minus, Plus, Slash, Star, LessDash, Colon,

        Not, Unequal, Equal, Greater, GreaterEqual, Less, LessEqual,

        Identifier, String, Integer, Float,

        And, Class, Else, False, For, If, Elif, Or, Print, Ret, Par, This, True, While, Begin, End, Do, Then, Input, Func, Null, Than, Set, To,

        NewLine,

        EOF
    };
}
