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
        //public string DefinitionTypeName { get; protected set; }
        public virtual CType Type { get; internal set; }

        internal Entity(CType type, string name)
        {
            //if (string.IsNullOrEmpty(typeName))
            //    throw new ArgumentNullException();
            //if (type == null)
            //    throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();
            Type = type;
            //this.DefinitionTypeName = typeName;
            Name = name;
        }

        public override string ToString()
        {
            return Type.Name + " " + Name;
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            //if (Type == null)
            //    Type = (CType)state.GetDeclaration(DefinitionTypeName);
            //else
            //    throw new InvalidOperationException();
            return false;
        }
    }
}
