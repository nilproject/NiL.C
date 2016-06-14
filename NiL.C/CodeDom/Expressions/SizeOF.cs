using System;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class SizeOf : Expression
    {
        public override CType ResultType
        {
            get
            {
                return first.ResultType.TargetType;
            }
        }

        public SizeOf(Expression first)
            : base(first, null)
        {
            if (!first.ResultType.IsPointer)
                throw new ArgumentException("Invalid argument type");
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return "*" + first;
        }
    }
}