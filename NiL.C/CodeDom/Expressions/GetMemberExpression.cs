using System;
using System.Collections.Generic;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class GetMemberExpression : Expression
    {
        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal GetMemberExpression(Expression obj, Expression fieldName)
            : base(obj, fieldName)
        {
        }

        public override string ToString()
        {
            var res = first.ToString();
            res += "." + second;
            return res;
        }
    }
}