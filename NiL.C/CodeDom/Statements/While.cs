using NiL.C.CodeDom.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class While : Statement
    {
        private CodeNode _condition;
        private CodeNode _body;

        private While()
        {

        }

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            if (!Parser.Validate(code, "while (", ref index)
                && !Parser.Validate(code, "while(", ref index))
                return null;

            Tools.SkipSpaces(code, ref index);

            var condition = Expressions.Expression.Parse(state, code, ref index);

            Tools.SkipSpaces(code, ref index);

            if (!Parser.Validate(code, ")", ref index))
                throw new SyntaxError();

            var body = Parser.Parse(state, code, ref index, 1);

            var result = new While
            {
                _condition = condition,
                _body = body
            };

            return result;
        }

        internal override void Emit(MethodBuilder method)
        {
            var generator = method.GetILGenerator();
            var exitLable = generator.DefineLabel();
            var loopLabel = generator.DefineLabel();

            generator.MarkLabel(loopLabel);

            var logical = _condition as ILogical;
            if (logical != null)
            {
                logical.SetLabelTarget(exitLable, true);
                _condition?.Emit(EmitMode.Get, method);
            }
            else
            {
                _condition?.Emit(EmitMode.Get, method);
                generator.Emit(OpCodes.Brfalse, exitLable);
            }

            _body?.Emit(EmitMode.SetOrNone, method);

            generator.Emit(OpCodes.Br, loopLabel);
            generator.MarkLabel(exitLable);
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            _condition?.Build(ref _condition, state);
            _body?.Build(ref _body, state);

            return false;
        }

        public override string ToString()
        {
            return "while (" + _condition + ")" + Environment.NewLine +
                        _body;
        }
    }
}
