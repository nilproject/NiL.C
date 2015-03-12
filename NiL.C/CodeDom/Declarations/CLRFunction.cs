﻿using System;
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
            returnType,
            methodInfo.GetParameters()
            .Select(x => new Argument(CLRType.Wrap(x.ParameterType), x.Name, x.Position) { IsVarArgArray = x.IsDefined(typeof(ParamArrayAttribute)) })
            .ToArray(),
            name)
        {
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");
            method = methodInfo;
            Body = new Statements.CodeBlock();
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
    }
}
