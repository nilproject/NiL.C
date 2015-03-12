using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Declarations
{
    internal sealed class GenericType : CType
    {
        public CType[] GenericParamaters { get; private set; }
        public int GenericParametersCount { get; internal set; }

        internal GenericType(string name)
            : base(name)
        {

        }
    }
}
