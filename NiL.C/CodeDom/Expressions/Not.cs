using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Not : Expression
    {
        public Not(Expression first)
            : base(first, null)
        {

        }

        public override string ToString()
        {
            return "~" + first;
        }
    }
}