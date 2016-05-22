using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NiL.C;
using System.IO;

namespace TestApp
{
    unsafe class Program
    {
        static void Main(string[] args)
        {
            Compiler.CompileAndSave(new StreamReader(new FileStream("program.c", FileMode.Open)).ReadToEnd(), "TestC", System.Reflection.Emit.PEFileKinds.ConsoleApplication);
            Assembly.LoadFile(Environment.CurrentDirectory + "\\testc.exe").GetModules()[0].GetMethod("main").Invoke(null, null);
            //Process.Start(new ProcessStartInfo(Environment.CurrentDirectory + "\\testc.exe") { UseShellExecute = false });
            Console.ReadKey(true);
        }

        private static void f(string format, params object[] o)
        {
            Console.Write(o);
        }

        unsafe void main()
        {
            void* p = (void*)0;
            f("", 1);
        }
    }
}
