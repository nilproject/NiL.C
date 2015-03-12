using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class Ternary : Expression
    {
        private Expression[] threads;

        public override bool IsContextIndependent
        {
            get
            {
                return base.IsContextIndependent
                    && (threads[0] is Constant || (threads[0] is Expression && threads[0].IsContextIndependent))
                    && (threads[1] is Constant || (threads[1] is Expression && threads[1].IsContextIndependent));
            }
        }

        public IList<CodeNode> Threads { get { return new ReadOnlyCollection<CodeNode>(threads); } }

        public Ternary(Expression first, Expression[] threads)
            : base(first, null)
        {
            this.threads = threads;
        }

        public override string ToString()
        {
            return "(" + first + " ? " + threads[0] + " : " + threads[1] + ")";
        }
    }
}