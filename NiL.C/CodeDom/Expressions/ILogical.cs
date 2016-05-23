using System.Reflection.Emit;

namespace NiL.C.CodeDom.Expressions
{
    internal interface ILogical
    {
        void SetLabelTarget(Label targetLabel, bool invert);
    }
}