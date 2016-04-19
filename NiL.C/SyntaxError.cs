using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.C
{
    public sealed class SyntaxError : Exception
    {
        public SyntaxError()
            : this("Unknown syntax error")
        {
        }

        public SyntaxError(string p)
            : base(p)
        {
        }
    }
}
