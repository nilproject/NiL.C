using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Declarations;

namespace NiL.C
{
    public static class Compiler
    {
        public static void CompileAndSave(string code, string name, PEFileKinds kind)
        {
            var space = new CodeDom.CSpace();
            space.Parse(code);
            
            var assm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), System.Reflection.Emit.AssemblyBuilderAccess.Save);
            var module = assm.DefineDynamicModule(name, kind == PEFileKinds.Dll ? name + ".dll" : name + ".exe", true);

            space.Build(module);
            space.Bind(module);

            module.CreateGlobalFunctions();
            var mainMethod = (space.Content.FirstOrDefault(x => x.Name == getEntryPointName(kind)) as Function).Method;

            if (mainMethod != null)
                assm.SetEntryPoint(mainMethod, kind);

            assm.Save(kind == PEFileKinds.Dll ? name + ".dll" : name + ".exe");
        }

        private static string getEntryPointName(PEFileKinds kind)
        {
            switch (kind)
            {
                case PEFileKinds.Dll:
                    return "dllmain";

                case PEFileKinds.ConsoleApplication:
                    return "main";

                case PEFileKinds.WindowApplication:
                    return "winmain";

                default:
                    throw new ArgumentException(nameof(kind));
            }
        }
    }
}
