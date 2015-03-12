using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Neg : Expression
    {
        public Neg(Expression first)
            : base(first, null)
        {

        }

        public override string ToString()
        {
            return "-" + first;
        }
    }
}