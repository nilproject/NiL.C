using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class ArrayExpression : Expression
    {
        private ArrayExpression()
        {

        }

        public override string ToString()
        {
            string res = "[";
            return res + ']';
        }
    }
}