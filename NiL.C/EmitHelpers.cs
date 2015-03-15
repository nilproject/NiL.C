using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C
{
    internal static class EmitHelpers
    {
        private static readonly MethodInfo WCSTRtoString = new Func<IntPtr, string>(Marshal.PtrToStringUni).Method;
        private static readonly MethodInfo CSTRtoString = new Func<IntPtr, string>(Marshal.PtrToStringAnsi).Method;

        internal static bool EmitConvert(ILGenerator generator, Type source, Type dest)
        {
            if (IsCompatible(source, dest))
                return false;
            if (source.IsValueType && dest == typeof(object))
            {
                generator.Emit(OpCodes.Box, source);
                return true;
            }
            if (dest == typeof(string))
            {
                if (source == typeof(char*)) // wchar_t
                {
                    generator.Emit(OpCodes.Call, WCSTRtoString);
                    return true;
                }

                if (source == typeof(byte*)) // char
                {
                    generator.Emit(OpCodes.Call, CSTRtoString);
                    return true;
                }
            }
            if (dest == typeof(object))
            {
                if (source.IsPointer)
                {
                    generator.Emit(OpCodes.Conv_I);
                    generator.Emit(OpCodes.Box, typeof(IntPtr));
                    return true;
                }
            }
            throw new NotImplementedException();
        }

        internal static bool IsCompatible(Type sourceType, Type destType)
        {
            if (sourceType == destType)
                return true;
            if (destType.IsValueType != sourceType.IsValueType)
                return false;
            if (sourceType.IsPointer != destType.IsPointer)
                return false;
            if (destType.IsAssignableFrom(sourceType))
                return true;
            else if (sourceType.IsPointer)
                return true;
            var sTypeCode = Type.GetTypeCode(sourceType);
            var dTypeCode = Type.GetTypeCode(destType);
            if (sTypeCode == TypeCode.Char)
                return dTypeCode == TypeCode.Char
                    || dTypeCode == TypeCode.Int16
                    || dTypeCode == TypeCode.UInt16;
            if ((sTypeCode >= TypeCode.SByte && sTypeCode <= TypeCode.UInt64)
                && (dTypeCode >= TypeCode.SByte && dTypeCode <= TypeCode.UInt64)
                && (((int)sTypeCode - 1) / 2 == ((int)dTypeCode - 1) / 2))
                return true;
            return false;
        }

        internal static int sizeOf(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return sizeof(bool);
                case TypeCode.Byte:
                    return sizeof(byte);
                case TypeCode.Char:
                    return sizeof(char);
                case TypeCode.Decimal:
                    return sizeof(decimal);
                case TypeCode.Double:
                    return sizeof(double);
                case TypeCode.Int16:
                    return sizeof(Int16);
                case TypeCode.Int32:
                    return sizeof(int);
                case TypeCode.Int64:
                    return sizeof(long);
                case TypeCode.SByte:
                    return sizeof(sbyte);
                case TypeCode.Single:
                    return sizeof(float);
                case TypeCode.UInt16:
                    return sizeof(ushort);
                case TypeCode.UInt32:
                    return sizeof(uint);
                case TypeCode.UInt64:
                    return sizeof(ulong);
                default:
                    return Marshal.SizeOf(type);
            }
        }

        internal static void EmitPushConstant_I4(ILGenerator generator, int value)
        {
            switch (value)
            {
                case 0:
                    {
                        generator.Emit(OpCodes.Ldc_I4_0);
                        break;
                    }
                case 1:
                    {
                        generator.Emit(OpCodes.Ldc_I4_1);
                        break;
                    }
                case 2:
                    {
                        generator.Emit(OpCodes.Ldc_I4_2);
                        break;
                    }
                case 3:
                    {
                        generator.Emit(OpCodes.Ldc_I4_3);
                        break;
                    }
                case 4:
                    {
                        generator.Emit(OpCodes.Ldc_I4_4);
                        break;
                    }
                case 5:
                    {
                        generator.Emit(OpCodes.Ldc_I4_5);
                        break;
                    }
                case 6:
                    {
                        generator.Emit(OpCodes.Ldc_I4_6);
                        break;
                    }
                case 7:
                    {
                        generator.Emit(OpCodes.Ldc_I4_7);
                        break;
                    }
                case 8:
                    {
                        generator.Emit(OpCodes.Ldc_I4_8);
                        break;
                    }
                default:
                    generator.Emit(OpCodes.Ldc_I4, (int)value);
                    break;
            }
        }
    }
}
