using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class MoreOrEqual : Less
    {
        public MoreOrEqual(Expression first, Expression second)
            : base(first, second)
        {

        }

        public override string ToString()
        {
            return "(" + first + " >= " + second + ")";
        }
    }
}