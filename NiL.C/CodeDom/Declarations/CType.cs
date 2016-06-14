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
        SChar,
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
        Function,
        Pointer
    }

    internal sealed class PointerType : CType
    {
        public override CTypeCode TypeCode
        {
            get
            {
                return CTypeCode.Pointer;
            }
        }

        public override bool IsPointer
        {
            get
            {
                return true;
            }
        }

        public override int Size
        {
            get
            {
                return IntPtr.Size;
            }
        }

        public PointerType(string itemTypeName)
            : base(itemTypeName + "*")
        {
        }
    }

    internal abstract class CType : Definition
    {
        protected CType refernceTypeCache = null;

        public virtual CTypeCode TypeCode
        {
            get
            {
                return CTypeCode.Object;
            }
        }

        public abstract bool IsPointer { get; }
        public virtual bool IsArray { get; internal set; }
        public virtual int ArrayLength { get; internal set; }
        public virtual CType TargetType { get; internal set; }
        public virtual bool IsGeneric { get; internal set; }
        public virtual bool IsCallable { get; internal set; }
        public virtual Entity Definition { get; protected set; }
        public abstract int Size { get; }

        internal CType(string name)
        {
            Name = name;
        }

        public virtual CType MakePointerType()
        {
            return refernceTypeCache ?? (refernceTypeCache = new PointerType(Name) { TargetType = this });
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

        protected override bool Build(ref CodeNode self, State state)
        {
            return false;
        }
    }
}
