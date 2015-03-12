
#define TYPE_SAFE

using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Mul : Expression
    {
        public Mul(Expression first, Expression second)
            : base(first, second)
        {

        }

        public override string ToString()
        {
            return "(" + first + " * " + second + ")";
        }
    }
}