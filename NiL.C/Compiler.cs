using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C
{
    public static class Compiler
    {
        public static void CompileAndSave(string code, string name, PEFileKinds kind)
        {
            var f = new CodeDom.CSpace();
            f.Process(code);
            var assm = f.Build(name, System.Reflection.Emit.AssemblyBuilderAccess.Save, kind);
            assm.Save(kind == PEFileKinds.Dll ? name + ".dll" : name + ".exe");
        }
    }
}
