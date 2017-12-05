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
        public IList<Definition> Content { get; private set; }

        internal CSpace()
        {
            this.Content = new List<Definition>();
            this.state = new State();
        }

        internal void Parse(string code)
        {
            code = Tools.RemoveComments(code, 0);

            int index = 0;

            while (code.Length > index)
            {
                while (code.Length > index && char.IsWhiteSpace(code[index])) index++;
                var item = (Definition)Parser.Parse(state, code, ref index, 0);
                if (item != null)
                    Content.Add(item);
            }
        }

        internal void Build(ModuleBuilder module)
        {
            for (var i = 0; i < Content.Count; i++)
            {
                var t = Content[i] as CodeNode;
                t.Build(ref t, state);
                Content[i] = (Definition)t;
            }

            Bind(module);
        }

        public void Bind(ModuleBuilder module)
        {
            for (var i = 0; i < Content.Count; i++)
                Content[i].Bind(module);

            for (var i = 0; i < Content.Count; i++)
                Content[i].Emit(module);
        }
    }
}
