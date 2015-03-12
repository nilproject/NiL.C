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
            { "char", new CLRType("char", typeof(sbyte)) },
            { "unsigned char", new CLRType("unsigned char", typeof(byte)) },
            { "wchar_t", new CLRType("wchar_t", typeof(char)) },
            { "short", new CLRType("short", typeof(short)) },
            { "unsigned short", new CLRType("unsigned short", typeof(ushort)) },
            { "int", new CLRType("int", typeof(int)) },
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
                typeof(System.Console).GetMethod("Write", new[] { typeof(string), typeof(object[]) })
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
