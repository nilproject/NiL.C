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

            var variable = (first as EntityAccessExpression).Declaration as Variable;
            if (variable != null)
            {
                variable.Pinned = true;
            }
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            var variable = (first as EntityAccessExpression).Declaration as Variable;
            if (variable != null)
            {
                first.Emit(EmitMode.GetPointer, method);
            }
            else
                throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "&" + first;
        }
    }
}