using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NiL.C.CodeDom.Declarations;

namespace NiL.C.CodeDom.Expressions
{
    internal interface IWantToGetType
    {
        void SetType(Type type);
    }
}
