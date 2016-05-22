using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using NiL.C.CodeDom;
using NiL.C.CodeDom.Declarations;

namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal class EntityAccessExpression : Expression
    {
        public Definition Declaration { get; private set; }

        public override CType ResultType
        {
            get
            {
                return (Declaration as Entity).Type;
            }
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        internal EntityAccessExpression(Definition declaration)
        {
            Declaration = declaration;
        }

        public override string ToString()
        {
            return Declaration.Name;
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            var prm = Declaration as Entity;
            if (prm == null)
                throw new NotImplementedException();
            
            prm.Emit(mode, method);
            /*if (mode == EmitMode.Get)
            {
                if (prm.VariableInfo != null) // переменная метода
                    method.GetILGenerator().Emit(OpCodes.Ldloc, prm.VariableInfo.LocalIndex);
                else if (prm.GetInfo(method.Module) != null) // глобальная переменная
                    method.GetILGenerator().Emit(OpCodes.Ldfld, (FieldInfo)prm.GetInfo(method.Module));
                else
                throw new NotImplementedException();
            }
            else
            {
                //method.GetILGenerator().Emit(OpCodes.Stloc, prm.VariableInfo.LocalIndex);
            }*/
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            return false;
        }
    }
}