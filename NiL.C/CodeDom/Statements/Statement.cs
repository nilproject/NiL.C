using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Statements
{
    internal abstract class Statement : CodeNode
    {
        internal abstract void Emit(System.Reflection.Emit.MethodBuilder method);

        internal override sealed void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            if (mode != EmitMode.SetOrNone)
                throw new ArgumentException();
            Emit(method);
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            throw new NotImplementedException();
        }
    }
}
