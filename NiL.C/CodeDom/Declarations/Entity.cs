using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal abstract class Entity : Definition
    {
        public virtual CType Type { get; internal set; }

        internal Entity(CType type, string name)
        {
            if (type == null)
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return Type.Name + " " + Name;
        }

        protected override bool PreBuild(ref CodeNode self, State state)
        {
            return false;
        }
    }
}
