using System;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class GetPointer : Expression
    {
        public override CType ResultType
        {
            get
            {
                return first.ResultType.MakePointerType();
            }
        }

        public GetPointer(Expression first)
            : base(first, null)
        {
            if (!(first is EntityAccessExpression))
                throw new ArgumentException("Can get variables for variables only");
            var prm = (first as EntityAccessExpression).Declaration as Variable;
            if (prm != null)
                prm.Pinned = true;
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            if ((first is EntityAccessExpression)
                && (first as EntityAccessExpression).Declaration is Entity)
            {
                
            }
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "&" + first;
        }
    }
}