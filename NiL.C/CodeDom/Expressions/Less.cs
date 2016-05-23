using NiL.C.CodeDom.Declarations;
using System;
using System.Reflection.Emit;

namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Less : Expression, ILogical
    {
        private Label _label;
        private bool _invert;

        internal Less(Expression first, Expression second)
            : base(first, second)
        {

        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            var firstType = first.ResultType;
            var secondType = second.ResultType;
            var fTypeCode = firstType.TypeCode;
            var sTypeCode = secondType.TypeCode;

            if (firstType.IsPointer && secondType.IsPointer)
                throw new ArgumentException("Can not process addition for pointers");
            if (!firstType.IsPointer && (fTypeCode <= CTypeCode.Void || fTypeCode >= CTypeCode.Object))
                throw new ArgumentException("Can not process addition with " + firstType);
            if (!secondType.IsPointer && (sTypeCode <= CTypeCode.Void || sTypeCode >= CTypeCode.Object))
                throw new ArgumentException("Can not process addition with " + secondType);

            first.Emit(EmitMode.Get, method);
            second.Emit(EmitMode.Get, method);

            if (mode == EmitMode.SetOrNone)
            {
                method.GetILGenerator().Emit(OpCodes.Pop);
                method.GetILGenerator().Emit(OpCodes.Pop);
            }
            else
            {
                if (_label != null)
                {
                    if (_invert)
                        method.GetILGenerator().Emit(OpCodes.Bge, _label);
                    else
                        method.GetILGenerator().Emit(OpCodes.Blt, _label);
                }
                else
                {
                    method.GetILGenerator().Emit(OpCodes.Clt);
                    if (_invert)
                    {
                        method.GetILGenerator().Emit(OpCodes.Ldc_I4_1);
                        method.GetILGenerator().Emit(OpCodes.Xor);
                    }
                }
            }
        }

        public override string ToString()
        {
            return "(" + first + " < " + second + ")";
        }

        public void SetLabelTarget(Label label, bool invert)
        {
            _label = label;
            _invert = invert;
        }
    }
}