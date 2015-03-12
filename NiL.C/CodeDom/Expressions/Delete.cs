using System;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Delete : Expression
    {
        public Delete(Expression first)
            : base(first, null)
        {

        }

        public override string ToString()
        {
            return "delete " + first;
        }
    }
}