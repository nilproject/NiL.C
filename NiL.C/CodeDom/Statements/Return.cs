using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Expressions;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class Return : Statement
    {
        private Expression argument;

        public Return(Expression arg)
        {
            argument = arg;
        }

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            if (!Parser.Validate(code, "return", ref index))
                return null;
            while (char.IsWhiteSpace(code[index])) index++;
            Expression arg = null;
            if (code[index] != ';')
                arg = (Expression)Expression.Parse(state, code, ref index);

            return new Return(arg);
        }

        internal override void Emit(System.Reflection.Emit.MethodBuilder method)
        {
            if (method.ReturnType != typeof(void) && argument == null)
                throw new ArgumentException("Function must return value");
            if (argument != null)
            {
                if (!EmitHelpers.IsCompatible((Type)argument.ResultType.GetInfo(method.Module), method.ReturnType)
                    && !EmitHelpers.Convertable((Type)argument.ResultType.GetInfo(method.Module), method.ReturnType))
                    throw new ArgumentException("Invalid return value (" + method + ")");

                argument.Emit(EmitMode.Get, method);
                EmitHelpers.EmitConvert(method.GetILGenerator(), (Type)argument.ResultType.GetInfo(method.Module), method.ReturnType);
            }

            method.GetILGenerator().Emit(System.Reflection.Emit.OpCodes.Ret);
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            if (argument != null)
                argument.Build(ref argument, state);

            return false;
        }
    }
}
