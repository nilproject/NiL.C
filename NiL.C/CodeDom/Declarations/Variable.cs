using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal sealed class Variable : Entity
    {
        private LocalVariableInfo info;
        public LocalVariableInfo VariableInfo
        {
            get
            {
                return info;
            }
        }

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
        public override int Size
        {
            get
            {
                return Type.Size;
            }
        }

        internal Variable(CType type, string name)
            : base(type, name)
        {

        }

        internal override void Bind(ModuleBuilder module)
        {
            throw new NotImplementedException();
        }

        internal override MemberInfo GetInfo(Module module)
        {
            throw new NotImplementedException();
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            if (info == null)
                throw new InvalidOperationException("Variable \"" + Name + "\" has not been defined");

            switch (mode)
            {
                case EmitMode.Get:
                    {
                        if (info.LocalIndex < 256)
                        {
                            switch (info.LocalIndex)
                            {
                                case 0:
                                    method.GetILGenerator().Emit(OpCodes.Ldloc_0);
                                    break;
                                case 1:
                                    method.GetILGenerator().Emit(OpCodes.Ldloc_1);
                                    break;
                                case 2:
                                    method.GetILGenerator().Emit(OpCodes.Ldloc_2);
                                    break;
                                case 3:
                                    method.GetILGenerator().Emit(OpCodes.Ldloc_3);
                                    break;
                                default:
                                    method.GetILGenerator().Emit(OpCodes.Ldloc_S, (byte)info.LocalIndex);
                                    break;
                            }
                        }
                        else
                            method.GetILGenerator().Emit(OpCodes.Ldloc, (short)info.LocalIndex);
                        break;
                    }
                case EmitMode.SetOrNone:
                    {
                        if (info.LocalIndex < 256)
                        {
                            switch (info.LocalIndex)
                            {
                                case 0:
                                    method.GetILGenerator().Emit(OpCodes.Stloc_0);
                                    break;
                                case 1:
                                    method.GetILGenerator().Emit(OpCodes.Stloc_1);
                                    break;
                                case 2:
                                    method.GetILGenerator().Emit(OpCodes.Stloc_2);
                                    break;
                                case 3:
                                    method.GetILGenerator().Emit(OpCodes.Stloc_3);
                                    break;
                                default:
                                    method.GetILGenerator().Emit(OpCodes.Stloc_S, (byte)info.LocalIndex);
                                    break;
                            }
                        }
                        else
                            method.GetILGenerator().Emit(OpCodes.Stloc, (short)info.LocalIndex);
                        break;
                    }
                case EmitMode.GetPointer:
                    {
                        if (info.LocalIndex < 256)
                            method.GetILGenerator().Emit(OpCodes.Ldloca_S, (byte)info.LocalIndex);
                        else
                            method.GetILGenerator().Emit(OpCodes.Ldloca, (short)info.LocalIndex);
                        break;
                    }
                default:
                    throw new ArgumentException();
            }
        }

        internal override void Emit(ModuleBuilder module)
        {
            throw new NotImplementedException();
        }

        internal override void Bind(MethodBuilder method)
        {
            if (info != null)
                throw new InvalidOperationException("Variable \"" + Name + "\" already defined");

            var local = method.GetILGenerator().DeclareLocal((Type)Type.GetInfo(method.Module), pinned);
            local.SetLocalSymInfo(Name);
            info = local;
        }
    }
}
