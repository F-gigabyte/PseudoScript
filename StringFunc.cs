using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PseudoScript
{
    class StringFunc
    {
        public static string RangedSubstr(string s, int from, int to)
        {
            return s.Substring(from, to - from);
        }
    }
}
