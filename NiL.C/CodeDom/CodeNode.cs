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
        internal T Prepare<T>(ref T self, State state) where T : CodeNode
        {
            var t = self as CodeNode;
            while (PreBuild(ref t, state)) ;
            return (T)t;
        }
        protected abstract bool PreBuild(ref CodeNode self, State state);
        internal abstract void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method);
    }
}
