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
            var info = cfunc.GetInfo(method.Module);
            if (cfunc == null)
                throw new ArgumentException();
            var prms = cfunc.Parameters;
            var generator = method.GetILGenerator();
            for (var i = 0; i < prms.Length; i++)
            {
                if (arguments.Length <= i)
                {
                    if ((prms[i].Type.GetInfo(method.Module) as Type).IsArray && prms[i].IsVarArgArray)
                    {
                        generator.Emit(OpCodes.Ldc_I4, 0);
                        generator.Emit(OpCodes.Newarr, (prms[i].Type.GetInfo(method.Module) as Type).GetElementType());
                    }
                    else
                        throw new NotImplementedException();
                }
                else
                {
                    if ((prms[i].Type.GetInfo(method.Module) as Type).IsArray && prms[i].IsVarArgArray)
                    {
                        var etype = (prms[i].Type.GetInfo(method.Module) as Type).GetElementType();
                        generator.Emit(OpCodes.Ldc_I4, arguments.Length - i);
                        generator.Emit(OpCodes.Newarr, etype);
                        var index = 0;
                        for (; i < arguments.Length; i++, index++)
                        {
                            generator.Emit(OpCodes.Dup);
                            generator.Emit(OpCodes.Ldc_I4, index);

                            arguments[i].Emit(EmitMode.Get, method);
                            var resultType = (Type)arguments[i].ResultType.GetInfo(method.Module);
                            if (!TypeTools.IsCompatible(resultType, etype)
                                && !TypeTools.EmitConvert(method.GetILGenerator(), resultType, etype))
                                throw new ArgumentException("Can not convert " + resultType + " to " + etype);
                            if (etype == typeof(object))
                                generator.Emit(OpCodes.Stelem_Ref);
                            else
                                generator.Emit(OpCodes.Stelem, etype);
                        }
                    }
                    else
                    {
                        //method.GetILGenerator().Emit(OpCodes.Ldstr, "test");
                        arguments[i].Emit(EmitMode.Get, method);
                        var resultType = (Type)arguments[i].ResultType.GetInfo(method.Module);
                        if (!TypeTools.IsCompatible(resultType, prms[i].Type.GetInfo(method.Module) as Type)
                            && !TypeTools.EmitConvert(method.GetILGenerator(), resultType, prms[i].Type.GetInfo(method.Module) as Type))
                            throw new ArgumentException("Can not convert " + resultType + " to " + prms[i].Type.GetInfo(method.Module));
                    }
                }
            }
            method.GetILGenerator().Emit(OpCodes.Call, info as MethodInfo);
        }
    }
}