﻿using System;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Indirection : Expression
    {
        public override CType ResultType
        {
            get
            {
                return first.ResultType.TargetType;
            }
        }

        public Indirection(Expression first)
            : base(first, null)
        {
            if (!first.ResultType.IsPointer)
                throw new ArgumentException("Invalid argument type");
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            first.Emit(EmitMode.Get, method);
            var targetType = (Type)first.ResultType.TargetType.GetInfo(method.Module);
            if (mode == EmitMode.Get)
                method.GetILGenerator().Emit(OpCodes.Ldobj, targetType);
            else if (mode == EmitMode.SetOrNone)
                method.GetILGenerator().Emit(OpCodes.Stobj, targetType);
        }

        public override string ToString()
        {
            return "*" + first;
        }
    }
}