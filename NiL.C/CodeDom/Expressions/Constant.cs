using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Constant : Expression
    {
        public override Declarations.CType ResultType
        {
            get
            {
                if (Value == null)
                    return (EmbeddedEntities.Declarations["void"] as CType).MakePointerType();
                if (Value.GetType() == typeof(string))
                    return (EmbeddedEntities.Declarations["wchar_t"] as CType).MakePointerType();
                if (Value.GetType() == typeof(byte[]))
                    return (EmbeddedEntities.Declarations["char"] as CType).MakePointerType();
                if (Value.GetType() == typeof(int))
                    return (EmbeddedEntities.Declarations["int"] as CType);
                throw new NotImplementedException(Value.GetType().ToString());
            }
        }
        public object Value { get; private set; }

        public Constant(object value)
            : base(null, null)
        {
            Value = value;
        }

        public override string ToString()
        {
            if (Value is char)
                return "'" + Value + "'";
            if (Value is string)
                return "\"" + Value + "\"";
            return Value.ToString();
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            if (Value == null)
                method.GetILGenerator().Emit(OpCodes.Ldnull);
            else if (Value.GetType() == typeof(byte[])
                  || Value.GetType() == typeof(sbyte[]))
            {
                var field = method.Module.GetField(this.GetHashCode().ToString("x2"), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (field == null)
                    field = (method.Module as ModuleBuilder).DefineInitializedData(this.GetHashCode().ToString("x2"), Value as byte[], System.Reflection.FieldAttributes.InitOnly);
                method.GetILGenerator().Emit(OpCodes.Ldsflda, field);
                /*
                 * В стеке будет указатель на структуру Array.
                 * В её заголовке два поля:
                 *      указатель на тип (4/8)
                 *      длина (4)
                 *      размерность (4)
                 * Итого: 12 / 16 байт
                 */
                /*if (IntPtr.Size == 4)
                {
                    method.GetILGenerator().Emit(OpCodes.Ldc_I4, 12);
                    method.GetILGenerator().Emit(OpCodes.Add);
                }
                else if (IntPtr.Size == 8)
                {
                    method.GetILGenerator().Emit(OpCodes.Ldc_I8, 16);
                    method.GetILGenerator().Emit(OpCodes.Add);
                }
                else
                    throw new NotImplementedException("Program is too old"); // (o_O)
                */
            }
            else
                switch (Type.GetTypeCode(Value.GetType()))
                {
                    case TypeCode.Int32:
                        {
                            method.GetILGenerator().Emit(OpCodes.Ldc_I4, (int)Value);
                            break;
                        }
                    case TypeCode.String:
                        {
                            method.GetILGenerator().Emit(OpCodes.Ldstr, Value.ToString());
                            if (IntPtr.Size == 4)
                            {
                                method.GetILGenerator().Emit(OpCodes.Ldc_I4, System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData + 4);
                                method.GetILGenerator().Emit(OpCodes.Add);
                            }
                            else if (IntPtr.Size == 8)
                            {
                                method.GetILGenerator().Emit(OpCodes.Ldc_I8, (long)System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData);
                                method.GetILGenerator().Emit(OpCodes.Add);
                            }
                            else
                                throw new NotImplementedException("Program is too old"); // (o_O)
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
        }
    }
}