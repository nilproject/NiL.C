using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Declarations;

namespace NiL.C
{
    internal static class EmbeddedEntities
    {
        public static readonly Dictionary<string, Definition> Declarations = new Dictionary<string, Definition>
        {
            { "char", new CLRType("char", typeof(byte)) },
            { "unsigned char", CLRType.Wrap(typeof(byte)) },
            { "signed char", new CLRType("signed char", typeof(sbyte)) },
            { "wchar_t", new CLRType("wchar_t", typeof(char)) },
            { "short", new CLRType("short", typeof(short)) },
            { "unsigned short", new CLRType("unsigned short", typeof(ushort)) },
            { "int", new CLRType("int", typeof(int)) },
            { "", new CLRType("<default>", typeof(int)) },
            { "unsigned int", new CLRType("unsigned int", typeof(uint)) },
            { "long long", new CLRType("long long", typeof(long)) },
            { "unsigned long long", new CLRType("unsigned long long", typeof(ulong)) },
            
            { "float", new CLRType("float", typeof(float)) },
            { "double", new CLRType("double", typeof(double)) },
            { "long double", new CLRType("long double", typeof(decimal)) },
            
            { "_Bool", new CLRType("_Bool", typeof(bool)) },
            { "void", new CLRType("void", typeof(void)) }
        };

        static EmbeddedEntities()
        {
            Declarations.Add("printf",
                CLRFunction.CreateFunction(
                "printf",
                typeof(NCRuntime.Stdio).GetMethod("printf")
                ));
            Declarations.Add("lmax",
                CLRFunction.CreateFunction(
                "lmax",
                typeof(NCRuntime.ExtMath).GetMethod("lmax")
                ));
            Declarations.Add("__wrtln",
                CLRFunction.CreateFunction(
                "__wrtln",
                typeof(NCRuntime.Debug).GetMethod("__wrtln")
                ));
        }

        public static CType GetTypeByCode(CTypeCode code)
        {
            switch (code)
            {
                case CTypeCode.Void:
                    return (CType)Declarations["void"];
                case CTypeCode.Int:
                    return (CType)Declarations["int"];
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
