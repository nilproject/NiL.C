using System;
using System.Collections.Generic;
using System.Reflection.Emit;



namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Incriment : Expression
    {
        public enum Type
        {
            Preincriment,
            Postincriment
        }

        public override bool IsContextIndependent
        {
            get
            {
                return false;
            }
        }

        public Incriment(Expression op, Type type)
            : base(op, type == Type.Postincriment ? op : null)
        {
            if (type > Type.Postincriment)
                throw new ArgumentException("type");
            if (op == null)
                throw new ArgumentNullException("op");
        }

        public override string ToString()
        {
            return second == null ? "++" + first : first + "++";
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            if (second != null)
            {
                first.Emit(EmitMode.Get, method);
                if (mode == EmitMode.Get)
                    method.GetILGenerator().Emit(OpCodes.Dup);
                method.GetILGenerator().Emit(OpCodes.Ldc_I4_1);
                method.GetILGenerator().Emit(OpCodes.Add);
                first.Emit(EmitMode.SetOrNone, method);
            }
            else
            {
                first.Emit(EmitMode.Get, method);
                method.GetILGenerator().Emit(OpCodes.Ldc_I4_1);
                method.GetILGenerator().Emit(OpCodes.Add);
                if (mode == EmitMode.Get)
                    method.GetILGenerator().Emit(OpCodes.Dup);
                first.Emit(EmitMode.SetOrNone, method);
            }
        }
    }
}