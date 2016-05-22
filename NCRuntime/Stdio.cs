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
            if (string.IsNullOrEmpty(format) || format.IndexOf('%') == -1)
                return format;

            StringBuilder result = new StringBuilder(format.Length);
            int offset = 0;
            for (int i = 0, argi = 0; i < format.Length; i++)
            {
                if (format[i] == '%' && format.Length > i + 1)
                {
                    i++;
                    var arg = args[argi++];
                    if (arg == null)
                        arg = 0xcccccccc;

                    var pinnedArg = GCHandle.Alloc(arg, GCHandleType.Pinned);
                    var addrArg = pinnedArg.AddrOfPinnedObject();
                    addrArg += offset;

                    int processedSize;

                    var size = arg is ValueType ? Marshal.SizeOf(arg) : IntPtr.Size;

                    try
                    {
                        switch (format[i])
                        {
                            case 'i': // signed int32
                                {
                                    processedSize = 4;
                                    result.Append(*(int*)addrArg);
                                    break;
                                }
                            default:
                                throw new ArgumentException("Unknown format specified: " + format[i]);
                        }
                    }
                    finally
                    {
                        pinnedArg.Free();
                    }

                    size -= offset;
                    if (size > processedSize)
                    {
                        offset += processedSize;
                        argi--;
                    }
                    else
                    {
                        offset = 0;
                    }
                }
                else
                {
                    result.Append(format[i]);
                }
            }

            return result.ToString();
        }

        public static void printf(byte* format, params object[] args)
        {
            var sformat = formatString(Marshal.PtrToStringAnsi((IntPtr)format), args);

            Console.Out.Write(sformat);
        }

        public static void scanf(byte* format, params void*[] args)
        {
            var sformat = Marshal.PtrToStringAnsi((IntPtr)format);

            if (sformat.Length == 0)
                return;

            int peak = 0;

            for (var i = 0; i < sformat.Length; i++)
            {
                peak = Console.In.Peek();

                switch (sformat[i])
                {
                    case '%':
                        {
                            i++;

                            if (sformat.Length == i)
                                goto default;

                            switch (sformat[i])
                            {
                                case 'u':
                                    {
                                        uint buf = 0;

                                        while (char.IsDigit((char)(peak = Console.In.Peek())))
                                        {
                                            buf *= 10;
                                            buf += (uint)(peak - '0');

                                            Console.In.Read();
                                        }

                                        *(uint*)args[0] = buf;

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        {
                            if (peak != sformat[i])
                                return;

                            break;
                        }
                }
            }

            for (var i = 0; i < Environment.NewLine.Length && Environment.NewLine[i] == (char)peak; i++)
            {
                Console.In.Read();
                peak = Console.In.Peek();
            }
        }
    }
}
