using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NiL.C.CodeDom.Declarations;

namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Call : Expression
    {
        private Expression[] arguments;
        public Expression[] Arguments { get { return arguments; } }

        public override CType ResultType
        {
            get
            {
                if (!(first is EntityAccessExpression))
                    throw new ArgumentException();
                var cfunc = (first as EntityAccessExpression).Declaration as CFunction;
                return cfunc.ReturnType;
            }
        }

        internal Call(Expression first, Expression[] arguments)
            : base(first, null)
        {
            this.arguments = arguments;
        }

        public override string ToString()
        {
            string res = first + "(";
            for (int i = 0; i < arguments.Length; i++)
            {
                res += arguments[i];
                if (i + 1 < arguments.Length)
                    res += ", ";
            }
            return res + ")";
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            if (!(first is EntityAccessExpression))
                throw new ArgumentException();
            var cfunc = (first as EntityAccessExpression).Declaration as CFunction;
            var info = cfunc.GetInfo(method.Module) as MethodInfo;
            if (cfunc == null)
                throw new ArgumentException();
            if ((info.ReturnType == typeof(void)) && (mode == EmitMode.Get))
                throw new InvalidOperationException();
            var prms = cfunc.Parameters;
            var generator = method.GetILGenerator();
            for (var i = 0; i < prms.Length; i++)
            {
                var targetType = prms[i].Type.GetInfo(method.Module) as Type;
                if (arguments.Length <= i)
                {
                    if (targetType.IsArray && prms[i].IsVarArgArray)
                    {
                        generator.Emit(OpCodes.Ldc_I4, 0);
                        generator.Emit(OpCodes.Newarr, targetType.GetElementType());
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if (targetType.IsArray && prms[i].IsVarArgArray)
                    {
                        var etype = targetType.GetElementType();
                        EmitHelpers.EmitPushConstant_I4(generator, arguments.Length - i);
                        generator.Emit(OpCodes.Newarr, etype);
                        var index = 0;
                        for (; i < arguments.Length; i++, index++)
                        {
                            generator.Emit(OpCodes.Dup);
                            EmitHelpers.EmitPushConstant_I4(generator, index);

                            if (arguments[i] is IWantToGetType)
                                (arguments[i] as IWantToGetType).SetType(etype);
                            arguments[i].Emit(EmitMode.Get, method);
                            var argType = (Type)arguments[i].ResultType.GetInfo(method.Module);
                            if (!EmitHelpers.IsCompatible(argType, etype)
                                && !EmitHelpers.EmitConvert(method.GetILGenerator(), argType, etype))
                                throw new ArgumentException("Can not convert " + argType + " to " + etype);
                            if (!etype.IsValueType)
                                generator.Emit(OpCodes.Stelem_Ref);
                            else
                            {
                                switch (Type.GetTypeCode(etype))
                                {
                                    case TypeCode.Byte:
                                    case TypeCode.SByte:
                                        {
                                            generator.Emit(OpCodes.Stelem_I1);
                                            break;
                                        }
                                    case TypeCode.Int16:
                                    case TypeCode.UInt16:
                                        {
                                            generator.Emit(OpCodes.Stelem_I2);
                                            break;
                                        }
                                    case TypeCode.Int32:
                                    case TypeCode.UInt32:
                                        {
                                            generator.Emit(OpCodes.Stelem_I4);
                                            break;
                                        }
                                    case TypeCode.Int64:
                                    case TypeCode.UInt64:
                                        {
                                            generator.Emit(OpCodes.Stelem_I8);
                                            break;
                                        }
                                    case TypeCode.Single:
                                        {
                                            generator.Emit(OpCodes.Stelem_R4);
                                            break;
                                        }
                                    case TypeCode.Double:
                                        {
                                            generator.Emit(OpCodes.Stelem_R8);
                                            break;
                                        }
                                    default:
                                        generator.Emit(OpCodes.Stelem, etype);
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (arguments[i] is IWantToGetType)
                            (arguments[i] as IWantToGetType).SetType(targetType);
                        arguments[i].Emit(EmitMode.Get, method);
                        var argType = (Type)arguments[i].ResultType.GetInfo(method.Module);
                        if (!EmitHelpers.IsCompatible(argType, targetType)
                            && !EmitHelpers.EmitConvert(method.GetILGenerator(), argType, targetType))
                            throw new ArgumentException("Can not convert " + argType + " to " + prms[i].Type.GetInfo(method.Module));
                    }
                }
            }
            generator.Emit(OpCodes.Call, info as MethodInfo);
            if (info.ReturnType != typeof(void) && mode != EmitMode.Get)
                generator.Emit(OpCodes.Pop);
        }

        protected override bool Prepare(ref CodeNode self, State state)
        {
            first.Prepare(ref first, state);
            for (int i = 0; i < arguments.Length; i++)
            {
                var a = arguments[i];
                a.Prepare(ref a, state);
                arguments[i] = a;
            }
            return false;
        }
    }
}