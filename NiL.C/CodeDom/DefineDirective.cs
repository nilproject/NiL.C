using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom
{
    internal sealed class DefineDirective : CodeNode
    {
        internal static ParseResult Parse(State state, string code, ref int index)
        {
            if (!Parser.Validate(code, "#define", ref index))
                return new ParseResult();

            while (char.IsWhiteSpace(code[index]) && code[index] != '\n' && code[index] != '\r') index++;

            var start = index;
            if (!Parser.ValidateName(code, ref index))
                return new ParseResult();

            var name = code.Substring(start, index - start);
            var value = "";

            while (char.IsWhiteSpace(code[index]) && code[index] != '\n' && code[index] != '\r') index++;
            
            if (code[index] != '\r' && code[index] != '\n')
            {
                start = index;
                if (!Parser.ValidateValue(code, ref index))
                    throw new SyntaxError();

                value = code.Substring(start, index - index);
            }

            state.DefineMacros(name, value);

            return new ParseResult() { IsParsed = true };
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            return false;
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
        }
    }
}
