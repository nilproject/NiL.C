using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal enum CTypeCode
    {
        Invalid = 0,
        Void,
        Char,
        UChar,
        WChar,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double,
        Object,
        Function
    }

    internal class CType : Definition
    {
        protected CType refernceTypeCache = null;

        public virtual CTypeCode TypeCode
        {
            get
            {
                return CTypeCode.Object;
            }
        }

        public virtual bool IsPointer { get { return TargetType != null; } }
        public virtual CType TargetType { get; internal set; }
        public virtual bool IsGeneric { get; internal set; }
        public virtual bool IsCallable { get; internal set; }
        public virtual Definition Definition { get; internal set; }

        internal CType(string name)
        {
            Name = name;
        }

        public virtual CType MakePointerType()
        {
            return refernceTypeCache ?? (refernceTypeCache = new CType(Name + "*") { TargetType = this });
        }

        public override string ToString()
        {
            return "Type \"" + Name + "\"";
        }

        internal override System.Reflection.MemberInfo GetInfo(Module module)
        {
            var type = module.GetType(Name);
            if (type != null)
                return type;
            Bind((ModuleBuilder)module);
            return module.GetType(Name);
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            throw new InvalidOperationException();
        }

        internal override void Bind(ModuleBuilder module)
        {
            throw new NotImplementedException();
        }

        internal override void Emit(ModuleBuilder module)
        {
            throw new NotImplementedException();
        }

        internal override void Bind(MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        protected override bool Prepare(ref CodeNode self, State state)
        {
            return false;
        }
    }
}
