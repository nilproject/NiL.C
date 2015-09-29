using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using NiL.C.CodeDom.Declarations;

namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Addition : Expression
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

        public Addition(Expression first, Expression second)
            : base(first, second)
        {
            if (first == null || second == null)
                throw new ArgumentNullException();
        }

        public override string ToString()
        {
            return "(" + first + " + " + second + ")";
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
            forPointerMul(method, secondType);
            second.Emit(EmitMode.Get, method);
            forPointerMul(method, firstType);
            method.GetILGenerator().Emit(OpCodes.Add);
            if (mode == EmitMode.SetOrNone)
                method.GetILGenerator().Emit(OpCodes.Pop);
        }

        private static void forPointerMul(System.Reflection.Emit.MethodBuilder method, CType type)
        {
            if (type.IsPointer)
            {
                if (type.TargetType.IsPointer)
                {
                    method.GetILGenerator().Emit(OpCodes.Sizeof, (Type)type.TargetType.GetInfo(method.Module));
                    method.GetILGenerator().Emit(OpCodes.Mul);
                }
                else
                {
                    var ttype = (Type)type.TargetType.GetInfo(method.Module);
                    var size = EmitHelpers.sizeOf(ttype);
                    if (size != 1)
                    {
                        method.GetILGenerator().Emit(OpCodes.Ldc_I4, size);
                        method.GetILGenerator().Emit(OpCodes.Mul);
                    }
                }
            }
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            first.Build(ref first, state);
            second.Build(ref second, state);
            return false;
        }
    }
}
