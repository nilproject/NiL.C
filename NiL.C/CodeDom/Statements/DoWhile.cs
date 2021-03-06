﻿using NiL.C.CodeDom.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class DoWhile : Statement
    {
        private CodeNode _condition;
        private CodeNode _body;

        private DoWhile()
        {

        }

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            if (!Parser.Validate(code, "do", ref index))
                return null;

            Tools.SkipSpaces(code, ref index);

            var body = Parser.Parse(state, code, ref index, 1);

            Tools.SkipSpaces(code, ref index);

            if (!Parser.Validate(code, "while (", ref index)
                && !Parser.Validate(code, "while(", ref index))
                throw new SyntaxError();

            Tools.SkipSpaces(code, ref index);

            var condition = Expression.Parse(state, code, ref index);

            if (!Parser.Validate(code, ")", ref index))
                throw new SyntaxError();

            var result = new DoWhile
            {
                _condition = condition,
                _body = body
            };

            return result;
        }

        internal override void Emit(MethodBuilder method)
        {
            var generator = method.GetILGenerator();
            var loopLabel = generator.DefineLabel();

            generator.MarkLabel(loopLabel);

            _body?.Emit(EmitMode.SetOrNone, method);

            var logical = _condition as ILogical;
            if (logical != null)
            {
                logical.SetLabelTarget(loopLabel, false);
                _condition?.Emit(EmitMode.Get, method);
            }
            else
            {
                _condition?.Emit(EmitMode.Get, method);
                generator.Emit(OpCodes.Brtrue, loopLabel);
            }
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            _condition?.Build(ref _condition, state);
            _body?.Build(ref _body, state);

            return false;
        }

        public override string ToString()
        {
            return "do" + Environment.NewLine + _body + Environment.NewLine + "while (" + _condition + ")";
        }
    }
}
