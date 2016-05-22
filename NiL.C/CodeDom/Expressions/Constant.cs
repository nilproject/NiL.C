using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Constant : Expression, IWantToGetType
    {
        private Type preferedType;

        public override Declarations.CType ResultType
        {
            get
            {
                if (Value == null)
                    return (EmbeddedEntities.Declarations["void"] as CType).MakePointerType();
                if (Value is char[])
                    return (EmbeddedEntities.Declarations["wchar_t"] as CType).MakePointerType();
                if (Value is string)
                {
                    // если получатель может работать с типом CLR, то отдаем ему этот тип
                    if (preferedType == typeof(string) || preferedType == typeof(object))
                        return CLRType.Wrap(typeof(string));
                    return (EmbeddedEntities.Declarations["wchar_t"] as CType).MakePointerType();
                }
                if (Value is byte[])
                {
                    //if (preferedType == typeof(object))
                    //    return CLRType.Wrap(typeof(byte[]));
                    return (EmbeddedEntities.Declarations["char"] as CType).MakePointerType();
                }
                if (Value is int)
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
            else if (Value.GetType().IsArray)
            {
                var field = method.Module.GetField(this.GetHashCode().ToString("x2"), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (field == null)
                {
                    byte[] bvalue;
                    switch (Type.GetTypeCode(Value.GetType().GetElementType()))
                    {
                        case TypeCode.Byte:
                            {
                                bvalue = Value as byte[];
                                break;
                            }
                        default:
                            throw new NotImplementedException();
                    }
                    field = (method.Module as ModuleBuilder).DefineInitializedData(this.GetHashCode().ToString("x2"),
                        bvalue,
                        System.Reflection.FieldAttributes.InitOnly);
                }

                method.GetILGenerator().Emit(OpCodes.Ldsflda, field);
            }
            else
            {
                switch (Type.GetTypeCode(Value.GetType()))
                {
                    case TypeCode.Int32:
                        {
                            EmitHelpers.EmitPushConstant_I4(method.GetILGenerator(), (int)Value);
                            break;
                        }
                    case TypeCode.String:
                        {
                            method.GetILGenerator().Emit(OpCodes.Ldstr, Value.ToString());
                            if (preferedType != typeof(string) && preferedType != typeof(object))
                            {
                                if (IntPtr.Size == 4)
                                {
                                    method.GetILGenerator().Emit(OpCodes.Ldc_I4, System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData);
                                    method.GetILGenerator().Emit(OpCodes.Add);
                                }
                                else if (IntPtr.Size == 8)
                                {
                                    method.GetILGenerator().Emit(OpCodes.Ldc_I8, (long)System.Runtime.CompilerServices.RuntimeHelpers.OffsetToStringData);
                                    method.GetILGenerator().Emit(OpCodes.Add);
                                }
                                else
                                    throw new NotImplementedException("Program is too old"); // (o_O)
                            }
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            return false;
        }

        public void SetType(Type type)
        {
            preferedType = type;
        }
    }
}