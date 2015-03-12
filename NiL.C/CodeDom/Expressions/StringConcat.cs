using System;
using System.Collections.Generic;
using System.Text;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal sealed class StringConcat : Expression
    {
        internal IList<Expression> sources;

        public override bool IsContextIndependent
        {
            get
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    if (!(sources[i] is Expression)
                        || !(sources[i] as Expression).IsContextIndependent)
                        return false;
                }
                return true;
            }
        }

        public StringConcat(IList<Expression> sources)
            : base(null, null)
        {
            if (sources.Count < 2)
                throw new ArgumentException("sources too short");
            this.sources = sources;
        }

        public override string ToString()
        {
            StringBuilder res = new StringBuilder("(", sources.Count * 10).Append(sources[0]);
            for (var i = 1; i < sources.Count; i++)
                res.Append(" + ").Append(sources[i]);
            return res.Append(")").ToString();
        }
    }
}