using System;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;



namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Assign : Expression
    {
        public Assign(Expression first, Expression second)
            : base(first, second)
        {
        }

        public override string ToString()
        {
            string f = first.ToString();
            if (f[0] == '(')
                f = f.Substring(1, f.Length - 2);
            string t = second.ToString();
            if (t[0] == '(')
                t = t.Substring(1, t.Length - 2);
            return "(" + f + " = " + t + ")";
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            if (first is EntityAccessExpression)
            {
                second.Emit(EmitMode.Get, method);
                if (mode == EmitMode.Get)
                    method.GetILGenerator().Emit(OpCodes.Dup);
                first.Emit(EmitMode.SetOrNone, method);
            }
            else if (first is Indirection)
            {
                first.Emit(EmitMode.GetPointer, method);
                second.Emit(EmitMode.Get, method);
                switch (first.ResultType.TypeCode)
                {
                    case CTypeCode.Char:
                    case CTypeCode.SChar:
                        method.GetILGenerator().Emit(OpCodes.Stind_I1);
                        break;
                    case CTypeCode.Short:
                    case CTypeCode.UShort:
                    case CTypeCode.WChar:
                        method.GetILGenerator().Emit(OpCodes.Stind_I2);
                        break;
                    case CTypeCode.Int:
                    case CTypeCode.UInt:
                        method.GetILGenerator().Emit(OpCodes.Stind_I4);
                        break;
                    case CTypeCode.Long:
                    case CTypeCode.ULong:
                        method.GetILGenerator().Emit(OpCodes.Stind_I8);
                        break;
                    case CTypeCode.Object:
                    case CTypeCode.Pointer:
                    case CTypeCode.Function:
                        method.GetILGenerator().Emit(OpCodes.Stind_Ref);
                        break;
                }

            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}