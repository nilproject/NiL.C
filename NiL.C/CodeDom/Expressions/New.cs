using System;
using System.Collections.Generic;

namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class New : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public New(Expression first, Expression[] arguments)
            : base(null, null)
        {

        }

        public override string ToString()
        {
            return "new " + first.ToString();
        }
    }
}