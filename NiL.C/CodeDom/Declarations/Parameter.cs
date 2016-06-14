using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal sealed class Parameter : Entity
    {
        private bool pinned = false;
        public bool Pinned
        {
            get
            {
                return pinned;
            }
            set
            {
                if (pinned && !value)
                    throw new InvalidOperationException();
                pinned = true;
            }
        }

        public int Index { get; private set; }
        public bool IsVarArgArray { get; internal set; }
        public override int Size
        {
            get
            {
                return Type.Size;
            }
        }

        internal Parameter(CType type, string name, int index)
            : base(type, name)
        {
            Index = index;
        }

        internal override void Bind(ModuleBuilder module)
        {
            throw new InvalidOperationException();
        }

        internal override MemberInfo GetInfo(Module module)
        {
            throw new InvalidOperationException();
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            switch (mode)
            {
                case EmitMode.Get:
                    {
                        method.GetILGenerator().Emit(OpCodes.Ldarg, (short)(Index - 1));
                        break;
                    }
                case EmitMode.SetOrNone:
                    {
                        method.GetILGenerator().Emit(OpCodes.Starg, (short)(Index - 1));
                        break;
                    }
                default:
                    throw new ArgumentException();
            }
        }

        internal override void Emit(ModuleBuilder module)
        {
            throw new InvalidOperationException();
        }

        internal override void Bind(MethodBuilder method)
        {
            method.DefineParameter(Index, ParameterAttributes.In, Name);
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            base.Build(ref self, state);
            state.DeclareSymbol(this);
            return false;
        }

        public override string ToString()
        {
            if (IsVarArgArray)
                return "params " + base.ToString();
            return base.ToString();
        }
    }
}
