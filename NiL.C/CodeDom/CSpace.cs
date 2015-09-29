using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Declarations;

namespace NiL.C.CodeDom
{
    internal class CSpace
    {
        private State state;
        public IList<CodeNode> Content { get; private set; }

        internal CSpace()
        {
            this.Content = new List<CodeNode>();
            this.state = new State();
        }

        internal void Process(string code)
        {
            code = Tools.RemoveComments(code, 0);

            int index = 0;

            while (code.Length > index)
            {
                while (code.Length > index && char.IsWhiteSpace(code[index])) index++;
                var item = Parser.Parse(state, code, ref index, 0);
                if (item != null)
                    Content.Add(item);
            }
        }

        internal AssemblyBuilder Build(string name, System.Reflection.Emit.AssemblyBuilderAccess mode, System.Reflection.Emit.PEFileKinds kind)
        {
            var assm = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), mode);
            var module = assm.DefineDynamicModule(name + "_Module", kind == PEFileKinds.Dll ? name + ".dll" : name + ".exe");
            for (var i = 0; i < Content.Count; i++)
            {
                var t = Content[i] as CodeNode;
                t.Build(ref t, state);
                Content[i] = (Definition)t;
            }
            for (var i = 0; i < Content.Count; i++)
                ((Definition)Content[i]).Bind(module);
            for (var i = 0; i < Content.Count; i++)
                ((Definition)Content[i]).Emit(module);
            module.CreateGlobalFunctions();

            if (kind != PEFileKinds.Dll)
            {
                var type = module.DefineType("<Generated>_entryPointWrapper", 
                    TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Abstract | TypeAttributes.Sealed);
                var entryPoint = type.DefineMethod(
                    "Main",
                    MethodAttributes.HideBySig | MethodAttributes.Static | MethodAttributes.Public,
                    typeof(void),
                    new[] { typeof(string) });

                //System.Reflection.Assembly.GetExecutingAssembly().Modules.First().GetMethod("main").Invoke(null, null);
                var mainMethod = module.GetMethod("main");
                //mainMethod.Invoke(null, null);
                var args = Expression.Parameter(typeof(string[]), "args");
                var modules = Expression.Parameter(typeof(IEnumerator<Module>), "modules");
                Expression.Lambda<Action<string[]>>(
                    Expression.Block(new[] { modules },
                    Expression.Assign(modules, Expression.Call(Expression.Property(Expression.Call(null, new Func<Assembly>(Assembly.GetExecutingAssembly).Method), "Modules"), "GetEnumerator", Type.EmptyTypes, null)),
                    Expression.Call(modules, typeof(IEnumerator).GetMethod("MoveNext", Type.EmptyTypes), null),
                    Expression.Call(Expression.Call(
                        Expression.Property(modules, "Current"),
                        "GetMethod", Type.EmptyTypes, Expression.Constant("main")),
                        "Invoke", Type.EmptyTypes, Expression.Constant(null), Expression.TypeAs(Expression.Constant(null), typeof(object[])))
                    ),
                    args).CompileToMethod(entryPoint);
                type.CreateType();
                assm.SetEntryPoint(entryPoint, kind);
            }

            return assm;
        }
    }
}
