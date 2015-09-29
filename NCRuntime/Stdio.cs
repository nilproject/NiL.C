using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NCRuntime
{
    public unsafe static class Stdio
    {
        private static string formatString(string format, params object[] args)
        {
            if (string.IsNullOrEmpty(format)
                || format.IndexOf('%') == -1)
                return format;
            StringBuilder result = new StringBuilder(format.Length);
            int offset = 0;
            for (int i = 0, argi = 0; i < format.Length; i++)
            {
                if (format[i] == '%' && format.Length > i + 1)
                {
                    i++;
                    var v = args[argi++];
                    if (v == null)
                        throw new ArgumentNullException();
                    var size = v is ValueType ? Marshal.SizeOf(v) : IntPtr.Size;
                    var gc = GCHandle.Alloc(v, GCHandleType.Pinned);
                    var buf = gc.AddrOfPinnedObject();
                    buf += offset;
                    size -= offset;
                    if (size > 4)
                    {
                        offset += 4;
                        argi--;
                    }
                    else
                        offset = 0;
                    try
                    {
                        Marshal.StructureToPtr(v, buf, false);
                        switch (format[i])
                        {
                            case 'i': // signed int32
                                {
                                    result.Append(*(int*)buf);
                                    break;
                                }
                            default:
                                throw new ArgumentException("Unknown format specified: " + format[i]);
                        }
                    }
                    finally
                    {
                        gc.Free();
                    }
                }
                else
                    result.Append(format[i]);
            }
            return result.ToString();
        }

        public static void printf(byte* format, params object[] args)
        {
            var sformat = formatString(Marshal.PtrToStringAnsi((IntPtr)format), args);
            System.Diagnostics.Debugger.Break();
        }
    }
}
