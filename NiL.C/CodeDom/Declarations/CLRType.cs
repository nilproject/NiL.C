using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal sealed class CLRType : CType
    {
        private readonly static Dictionary<Type, CType> wrapCache = new Dictionary<Type, CType>();

        private Type proto;

        public override bool IsComplete
        {
            get
            {
                return true;
            }
            protected set
            {

            }
        }

        public override CTypeCode TypeCode
        {
            get
            {
                if (proto == typeof(void))
                    return CTypeCode.Void;
                switch (Type.GetTypeCode(proto))
                {
                    case System.TypeCode.Byte: // UChar
                        return CTypeCode.UChar;
                    case System.TypeCode.SByte:
                        return CTypeCode.Char;
                    case System.TypeCode.UInt16:
                        return CTypeCode.UShort;
                    case System.TypeCode.Int16:
                        return CTypeCode.Short;
                    case System.TypeCode.UInt32:
                        return CTypeCode.UInt;
                    case System.TypeCode.Int32:
                        return CTypeCode.Int;
                    case System.TypeCode.UInt64:
                        return CTypeCode.ULong;
                    case System.TypeCode.Int64:
                        return CTypeCode.Long;
                    case System.TypeCode.Single:
                        return CTypeCode.Float;
                    case System.TypeCode.Double:
                        return CTypeCode.Double;
                    default:
                        return CTypeCode.Object;
                }
            }
        }

        public override bool IsPointer
        {
            get
            {
                return proto.IsPointer;
            }
        }

        public override CType TargetType
        {
            get
            {
                CType res = null;
                if (wrapCache.TryGetValue(proto.GetElementType(), out res))
                    return res;
                return MakePointerType();
            }
            internal set
            {

            }
        }

        internal CLRType(string alias, Type proto)
            : base(alias)
        {
            this.proto = proto;
            wrapCache.Add(proto, this);
        }

        internal static CType Wrap(Type type)
        {
            CType res = null;
            if (wrapCache.TryGetValue(type, out res))
                return res;
            return new CLRType(type.Name, type);
        }

        internal override void Bind(ModuleBuilder module)
        {
            throw new InvalidOperationException();
        }

        internal override MemberInfo GetInfo(Module module)
        {
            return proto;
        }

        public override CType MakePointerType()
        {
            if (refernceTypeCache == null)
                refernceTypeCache = CLRType.Wrap(proto.MakePointerType());
            return refernceTypeCache;
        }
    }
}
