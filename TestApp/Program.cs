using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NiL.C;

namespace TestApp
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            Compiler.CompileAndSave(@"
int mul(a, b)
int a;
int b;
{
    return a + b;
}

int sum(int a, int b)
{
    return a + b;
}

int main()
{
    __wrtln(sum(1,2));
    __wrtln(mul(3,4));
    
}
", "TestC", System.Reflection.Emit.PEFileKinds.ConsoleApplication);
            Assembly.LoadFile(Environment.CurrentDirectory + "\\testc.exe").GetModules()[0].GetMethod("main").Invoke(null, null);
        }

        struct Struct
        {
            public int i;
            public char* t;
        }

        unsafe void main()
        {
            var s = new Struct();
            var ps = &s;
            NCRuntime.ExtMath.lmax(1, 2, 3, 4, 5);
        }
    }
}
