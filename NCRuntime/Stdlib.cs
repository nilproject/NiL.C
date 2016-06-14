using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NCRuntime
{
    public unsafe static class Stdlib
    {
        public static void* malloc(int size)
        {
            return (void*)Marshal.AllocHGlobal(size);
        }

        public static void* calloc(int count, int itemSize)
        {
            var result = (byte*)malloc(count * itemSize);

            for (var i = 0; i < count * itemSize; i++)
                result[i] = 0;

            return result;
        }
    }
}
