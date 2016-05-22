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
        SetOrNone,
        GetPointer
    }

    internal abstract class CodeNode
    {
        internal void Build<T>(ref T self, State state) where T : CodeNode
        {
            var t = self as CodeNode;
            while (t.Build(ref t, state)) ;
            self = (T)t;
        }
        protected abstract bool Build(ref CodeNode self, State state);
        internal abstract void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method);
    }
}
