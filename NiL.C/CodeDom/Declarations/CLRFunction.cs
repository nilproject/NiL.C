using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal sealed class CLRFunction : CFunction
    {
        private MethodInfo method;

        public override bool IsComplete
        {
            get
            {
                return method != null;
            }
            protected set
            {

            }
        }

        private CLRFunction(CType returnType, string name, MethodInfo methodInfo)
            : base(
            returnType.Name,
            methodInfo.GetParameters()
            .Select(x => new Argument(CLRType.Wrap(x.ParameterType), x.Name, x.Position) { IsVarArgArray = x.IsDefined(typeof(ParamArrayAttribute)) })
            .ToArray(),
            name)
        {
            method = methodInfo;
            Body = new Statements.CodeBlock();
            this.ReturnType = returnType;
        }

        public static CLRFunction CreateFunction(string alias, MethodInfo method)
        {
            CType returntype = null;
            switch (System.Type.GetTypeCode(method.ReturnType))
            {
                case TypeCode.Empty:
                    {
                        returntype = (CType)EmbeddedEntities.Declarations["void"];
                        break;
                    }
                case TypeCode.Object:
                    {
                        if (method.ReturnType == typeof(void))
                            goto case TypeCode.Empty;
                        goto default;
                    }
                default:
                    throw new NotImplementedException(method.ReturnType.ToString());
            }
            return new CLRFunction(returntype, alias, method);
        }

        internal override void Bind(ModuleBuilder module)
        {
            throw new InvalidOperationException();
        }

        internal override MemberInfo GetInfo(Module module)
        {
            return method;
        }

        protected override bool Prepare(ref CodeNode self, State state)
        {
            return false;
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            throw new InvalidOperationException();
        }

        internal override void Bind(MethodBuilder method)
        {
            throw new InvalidOperationException();
        }

        internal override void Emit(ModuleBuilder module)
        {
            throw new InvalidOperationException();
        }
    }
}
