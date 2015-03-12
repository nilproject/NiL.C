using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NiL.C.CodeDom
{
    public sealed class SyntaxError : Exception
    {
        public SyntaxError(string p)
            : base(p)
        {
        }
    }
}
