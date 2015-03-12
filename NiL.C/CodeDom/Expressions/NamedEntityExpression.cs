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

        public string Name { get; private set; }

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

        internal EntityAccessExpression(string name)
        {
            Name = name;
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
            if (mode == EmitMode.Get)
            {
                prm.Emit(EmitMode.Get, method);
                /*if (prm.VariableInfo != null) // переменная метода
                    method.GetILGenerator().Emit(OpCodes.Ldloc, prm.VariableInfo.LocalIndex);
                else if (prm.GetInfo(method.Module) != null) // глобальная переменная
                    method.GetILGenerator().Emit(OpCodes.Ldfld, (FieldInfo)prm.GetInfo(method.Module));
                else
                throw new NotImplementedException();*/
            }
            else
            {
                prm.Emit(EmitMode.SetOrNone, method);
                //method.GetILGenerator().Emit(OpCodes.Stloc, prm.VariableInfo.LocalIndex);
            }
        }

        protected override bool Prepare(ref CodeNode self, State state)
        {
            if (Declaration != null)
                throw new InvalidOperationException();
            Declaration = state.GetDeclaration(Name);
            return false;
        }
    }
}