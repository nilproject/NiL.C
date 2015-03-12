using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NiL.C;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Compiler.CompileAndSave(@"
void main(void)
{
    printf(""{0}"", sum(1,2));
}

int sum(int a, int b)
{
    return a + b;
}
            ", "TestC", System.Reflection.Emit.PEFileKinds.ConsoleApplication);
            //var method = assembly.GetType("<Generated>_entryPointWrapper").GetMethod("Main", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            //method.Invoke(null, new string[][] { null });
        }

        unsafe void main()
        {
            var a = 1;
            var b = 2;
            a++;
            var pa = &a;
            var pb = &b;
            pa = pa + 1;
            var ppa = &pa;
            ppa += a;
        }
    }
}
