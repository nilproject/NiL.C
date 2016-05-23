using NiL.C.CodeDom.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class IfElse : Statement
    {
        private CodeNode _condition;
        private CodeNode _body;
        private CodeNode _else;

        private IfElse()
        {

        }

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            if (!Parser.Validate(code, "if (", ref index)
                && !Parser.Validate(code, "if(", ref index))
                return null;

            Tools.SkipSpaces(code, ref index);

            var condition = Expressions.Expression.Parse(state, code, ref index);

            Tools.SkipSpaces(code, ref index);

            if (!Parser.Validate(code, ")", ref index))
                throw new SyntaxError();

            var body = Parser.Parse(state, code, ref index, 1);

            var pindex = index;
            Tools.SkipSpaces(code, ref index);
            CodeNode elseBody = null;
            if (Parser.Validate(code, "else", ref index))
            {
                elseBody = Parser.Parse(state, code, ref index, 1);
                if (elseBody == null)
                    throw new SyntaxError();
            }
            else
            {
                index = pindex;
            }

            var result = new IfElse
            {
                _condition = condition,
                _body = body,
                _else = elseBody
            };

            return result;
        }

        internal override void Emit(MethodBuilder method)
        {
            var generator = method.GetILGenerator();
            var exitLable = generator.DefineLabel();
            var elseBypass = _else != null ? generator.DefineLabel() : default(Label);

            var logical = _condition as ILogical;
            if (logical != null)
            {
                logical.SetLabelTarget(exitLable, true);
                _condition.Emit(EmitMode.Get, method);
            }
            else
            {
                _condition.Emit(EmitMode.Get, method);
                generator.Emit(OpCodes.Brfalse, exitLable);
            }

            _body.Emit(EmitMode.SetOrNone, method);

            if (_else != null)
            {
                generator.Emit(OpCodes.Br, elseBypass);
                generator.MarkLabel(exitLable);

                _else.Emit(EmitMode.SetOrNone, method);

                generator.MarkLabel(elseBypass);
            }
            else
            {
                generator.MarkLabel(exitLable);
            }
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            _condition?.Build(ref _condition, state);
            _body?.Build(ref _body, state);
            _else?.Build(ref _else, state);

            return false;
        }

        public override string ToString()
        {
            return "if (" + _condition + ")" + Environment.NewLine +
                        _body
                 + (_else != null ? Environment.NewLine + "else" + Environment.NewLine +
                        _else : "");
        }
    }
}
