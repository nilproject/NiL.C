using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal sealed class CLRFunction : Function
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
            returnType,
            methodInfo.GetParameters()
            .Select(x => new Parameter(CLRType.Wrap(x.ParameterType), x.Name, x.Position) { IsVarArgArray = x.IsDefined(typeof(ParamArrayAttribute)) })
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
            var type = method.ReturnType;

            var pointerDepth = 0;
            while(type.IsPointer)
            {
                pointerDepth++;
                type = type.GetElementType();
            }

            switch (System.Type.GetTypeCode(type))
            {
                case TypeCode.Empty:
                    {
                        returntype = (CType)EmbeddedEntities.Declarations["void"];
                        break;
                    }
                case TypeCode.Int32:
                    {
                        returntype = (CType)EmbeddedEntities.Declarations["int"];
                        break;
                    }
                case TypeCode.Object:
                    {
                        if (type == typeof(void))
                            goto case TypeCode.Empty;

                        goto default;
                    }
                default:
                    throw new NotImplementedException(type.ToString());
            }

            while (pointerDepth-- > 0)
                returntype = returntype.MakePointerType();

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

        protected override bool Build(ref CodeNode self, State state)
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
