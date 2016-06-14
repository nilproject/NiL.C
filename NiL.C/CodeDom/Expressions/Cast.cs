using System;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Cast : Expression
    {
        private CType _type;

        public override CType ResultType
        {
            get
            {
                return _type;
            }
        }

        public Cast(Expression first, CType type)
            : base(first, null)
        {
            _type = type;
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            first.Emit(EmitMode.Get, method);

            switch (_type.TypeCode)
            {
                case CTypeCode.SChar:
                    method.GetILGenerator().Emit(OpCodes.Conv_I1);
                    break;
                case CTypeCode.Char:
                    method.GetILGenerator().Emit(OpCodes.Conv_U1);
                    break;
                case CTypeCode.Short:
                    method.GetILGenerator().Emit(OpCodes.Conv_I2);
                    break;
                case CTypeCode.WChar:
                case CTypeCode.UShort:
                    method.GetILGenerator().Emit(OpCodes.Conv_U2);
                    break;
                case CTypeCode.Int:
                    method.GetILGenerator().Emit(OpCodes.Conv_I4);
                    break;
                case CTypeCode.UInt:
                    method.GetILGenerator().Emit(OpCodes.Conv_U4);
                    break;
                case CTypeCode.Long:
                    method.GetILGenerator().Emit(OpCodes.Conv_I8);
                    break;
                case CTypeCode.ULong:
                    method.GetILGenerator().Emit(OpCodes.Conv_U8);
                    break;

                case CTypeCode.Float:
                    method.GetILGenerator().Emit(OpCodes.Conv_R4);
                    break;
                case CTypeCode.Double:
                    method.GetILGenerator().Emit(OpCodes.Conv_R8);
                    break;

                case CTypeCode.Pointer:
                case CTypeCode.Function:
                    if (first.ResultType.IsPointer)
                        break;
                    else
                        throw new InvalidCastException();
            }
        }

        public override string ToString()
        {
            return "(" + _type.Name + ")" + first;
        }
    }
}