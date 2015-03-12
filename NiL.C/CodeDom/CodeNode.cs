using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom
{
    internal enum EmitMode
    {
        Get,
        SetOrNone
    }

    internal abstract class CodeNode
    {
        internal void Prepare<T>(ref T self, State state) where T : CodeNode
        {
            var t = self as CodeNode;
            while (t.Prepare(ref t, state)) ;
            self = (T)t;
        }
        protected abstract bool Prepare(ref CodeNode self, State state);
        internal abstract void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method);
    }
}
