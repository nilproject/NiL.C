using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class LogicalNot : Expression
    {
        public LogicalNot(Expression first)
            : base(first, null)
        {

        }

        public override string ToString()
        {
            return "!" + first;
        }
    }
}