﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class CodeBlock : Statement
    {
        public CodeNode[] Lines { get; private set; }

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            if (code[index] != '{')
                return null;
            do index++; while (char.IsWhiteSpace(code[index]));
            var lines = new List<CodeNode>();

            using (state.Scope)
            {
                while (code[index] != '}')
                {
                    var line = Parser.Parse(state, code, ref index, 1);
                    if (line != null)
                    {
                        lines.Add(line);
                    }

                    while (char.IsWhiteSpace(code[index])) index++;
                }
            }

            index++;
            return new CodeBlock() { Lines = lines.ToArray() };
        }

        internal override void Emit(MethodBuilder method)
        {
            for (var i = 0; i < Lines.Length; i++)
                Lines[i].Emit(EmitMode.SetOrNone, method);
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            using (state.Scope)
            {
                for (var i = 0; i < Lines.Length; i++)
                    Lines[i].Build(ref Lines[i], state);
                return false;
            }
        }
    }
}
