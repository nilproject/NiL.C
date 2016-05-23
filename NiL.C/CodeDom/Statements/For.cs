using NiL.C.CodeDom.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class For : Statement
    {
        private CodeNode _initializer;
        private CodeNode _condition;
        private CodeNode _post;
        private CodeNode _body;

        private For()
        {

        }

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            if (!Parser.Validate(code, "for (", ref index)
                && !Parser.Validate(code, "for(", ref index))
                return null;

            Tools.SkipSpaces(code, ref index);

            var initializer = VariableDefinition.Parse(state, code, ref index)
                           ?? Expressions.Expression.Parse(state, code, ref index);
            
            Tools.SkipSpaces(code, ref index);

            if (!Parser.Validate(code, ";", ref index))
                throw new SyntaxError();

            Tools.SkipSpaces(code, ref index);

            var condition = Expressions.Expression.Parse(state, code, ref index);

            Tools.SkipSpaces(code, ref index);

            if (!Parser.Validate(code, ";", ref index))
                throw new SyntaxError();

            Tools.SkipSpaces(code, ref index);

            var post = Expressions.Expression.Parse(state, code, ref index);

            Tools.SkipSpaces(code, ref index);

            if (!Parser.Validate(code, ")", ref index))
                throw new SyntaxError();

            var body = Parser.Parse(state, code, ref index, 1);

            var result = new For
            {
                _initializer = initializer,
                _condition = condition,
                _post = post,
                _body = body
            };

            return result;
        }

        internal override void Emit(MethodBuilder method)
        {
            var generator = method.GetILGenerator();
            var exitLable = generator.DefineLabel();
            var loopLabel = generator.DefineLabel();

            _initializer.Emit(EmitMode.SetOrNone, method);

            generator.MarkLabel(loopLabel);

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

            _post.Emit(EmitMode.SetOrNone, method);

            generator.Emit(OpCodes.Br, loopLabel);
            generator.MarkLabel(exitLable);
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            _initializer?.Build(ref _initializer, state);
            _condition?.Build(ref _condition, state);
            _post?.Build(ref _post, state);
            _body?.Build(ref _body, state);

            return false;
        }

        public override string ToString()
        {
            return "for (" + _initializer + "; " + _condition + "; " + _post + ")" + Environment.NewLine +
                        _body;
        }
    }
}
