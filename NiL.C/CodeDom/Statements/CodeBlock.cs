using System;
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

        internal static ParseResult Parse(State state, string code, ref int index)
        {
            if (code[index] != '{')
                return new ParseResult();
            do index++; while (char.IsWhiteSpace(code[index]));
            var lines = new List<CodeNode>();
            while (code[index] != '}')
            {
                var line = Parser.Parse(state, code, ref index, 1);
                if (line != null)
                {
                    lines.Add(line);
                }
                while (char.IsWhiteSpace(code[index])) index++;
            }
            index++;
            return new ParseResult()
            {
                IsParsed = true,
                Statement = new CodeBlock() { Lines = lines.ToArray() }
            };
        }

        internal override void Emit(MethodBuilder method)
        {
            for (var i = 0; i < Lines.Length; i++)
                Lines[i].Emit(EmitMode.SetOrNone, method);
        }

        protected override bool PreBuild(ref CodeNode self, State state)
        {
            for (var i = 0; i < Lines.Length; i++)
                Lines[i].Prepare(ref Lines[i], state);
            return false;
        }
    }
}
