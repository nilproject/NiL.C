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
int f(a, b)
int a;
int b;
{
    return a + b * 2;
}

int main()
{
    __wrtln(f(1, 2));
    
}
", "TestC", System.Reflection.Emit.PEFileKinds.ConsoleApplication);
            //Assembly.LoadFile(Environment.CurrentDirectory + "\\testc.exe").GetModules()[0].GetMethod("main").Invoke(null, null);
            Process.Start(new ProcessStartInfo(Environment.CurrentDirectory + "\\testc.exe") { UseShellExecute = false });
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
