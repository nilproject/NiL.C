using System;



namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Decrement : Expression
    {
        public enum Type
        {
            Predecriment,
            Postdecriment
        }

        public Decrement(Expression op, Type type)
            : base(op, type == Type.Postdecriment ? op : null)
        {
            if (type > Type.Postdecriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
        }

        public override string ToString()
        {
            return first != null ? "--" + first : second + "--";
        }
    }
}