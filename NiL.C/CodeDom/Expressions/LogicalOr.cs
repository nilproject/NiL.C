using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class LogicalOr : Expression
    {
        public LogicalOr(Expression first, Expression second)
            : base(first, second)
        {

        }

        public override string ToString()
        {
            return "(" + first + " || " + second + ")";
        }
    }
}