using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal abstract class Definition : CodeNode
    {
        public virtual bool IsComplete { get; protected set; }
        public virtual string Name { get; protected set; }

        internal abstract void Bind(ModuleBuilder module);
        internal abstract void Emit(ModuleBuilder module);
        internal abstract void Bind(MethodBuilder method);
        internal abstract MemberInfo GetInfo(Module module);
    }
}
