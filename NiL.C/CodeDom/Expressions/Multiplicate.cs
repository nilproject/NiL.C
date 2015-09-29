
#define TYPE_SAFE

using System;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Multiplicate : Expression
    {
        public override Declarations.CType ResultType
        {
            get
            {
                var ftypecode = first.ResultType.TypeCode;
                var stypecode = second.ResultType.TypeCode;
                if (ftypecode <= CTypeCode.Void)
                    throw new ArgumentException("Invalid operand type");
                if (stypecode <= CTypeCode.Void)
                    throw new ArgumentException("Invalid operand type");
                return EmbeddedEntities.GetTypeByCode((CTypeCode)Math.Max(Math.Max((int)ftypecode, (int)stypecode), (int)CTypeCode.Int));
            }
        }

        public Multiplicate(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            var firstType = first.ResultType;
            var secondType = second.ResultType;
            var fTypeCode = firstType.TypeCode;
            var sTypeCode = secondType.TypeCode;
            if (firstType.IsPointer && secondType.IsPointer)
                throw new ArgumentException("Can not process addition for pointers");
            if (!firstType.IsPointer && (fTypeCode <= CTypeCode.Void || fTypeCode >= CTypeCode.Object))
                throw new ArgumentException("Can not process addition with " + firstType);
            if (!secondType.IsPointer && (sTypeCode <= CTypeCode.Void || sTypeCode >= CTypeCode.Object))
                throw new ArgumentException("Can not process addition with " + secondType);
            first.Emit(EmitMode.Get, method);
            second.Emit(EmitMode.Get, method);
            method.GetILGenerator().Emit(OpCodes.Mul);
            if (mode == EmitMode.SetOrNone)
                method.GetILGenerator().Emit(OpCodes.Pop);
        }

        public override string ToString()
        {
            return "(" + first + " * " + second + ")";
        }
    }
}