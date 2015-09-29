using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCRuntime
{
    public unsafe static class Debug
    {
        public static void __wrtln(object m)
        {
            Console.WriteLine(m);
            //while (*m != 0)
            //    Console.Write(*m++);
        }
    }
}
