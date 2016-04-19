using System;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using NiL.C.CodeDom.Declarations;

namespace NiL.C
{
    public sealed class CodeCoordinates
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public int Length { get; private set; }

        public CodeCoordinates(int line, int column, int length)
        {
            Line = line;
            Column = column;
            Length = length;
        }

        public override string ToString()
        {
            return "(" + Line + ": " + Column + "*" + Length + ")";
        }

        public static CodeCoordinates FromTextPosition(string text, int position, int length)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (position < 0)
                throw new ArgumentOutOfRangeException("position");
            int line = 1;
            int column = 1;
            for (int i = 0; i < position; i++)
            {
                if (text[i] == '\n')
                {
                    column = 0;
                    line++;
                    if (text[i + 1] == '\r')
                        i++;
                }
                else if (text[i] == '\r')
                {
                    column = 0;
                    line++;
                    if (text[i + 1] == '\n')
                        i++;
                }
                column++;
            }
            return new CodeCoordinates(line, column, length);
        }
    }
    /// <summary>
    /// Содержит функции, используемые на разных этапах выполнения скрипта.
    /// </summary>
    public static class Tools
    {
        [Flags]
        public enum ParseNumberOptions
        {
            None = 0,
            RaiseIfOctal = 1,
            ProcessOctal = 2,
            AllowFloat = 4,
            AllowAutoRadix = 8,
            Default = 2 + 4 + 8
        }

        internal static readonly char[] TrimChars = new[] { '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u0020', '\u00A0', '\u1680', '\u180E', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000', '\uFEFF' };

        internal static readonly char[] NumChars = new[] 
        { 
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' 
        };
        internal static readonly string[] NumString = new[] 
		{ 
            "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15"
		};

        //internal static readonly string[] charStrings = (from x in Enumerable.Range(char.MinValue, char.MaxValue) select ((char)x).ToString()).ToArray();

        internal sealed class _ForcedEnumerator<T> : IEnumerator<T>
        {
            private int index;
            private IEnumerable<T> owner;
            private IEnumerator<T> parent;

            private _ForcedEnumerator(IEnumerable<T> owner)
            {
                this.owner = owner;
                this.parent = owner.GetEnumerator();
            }

            public static _ForcedEnumerator<T> create(IEnumerable<T> owner)
            {
                return new _ForcedEnumerator<T>(owner);
            }

            #region Члены IEnumerator<T>

            public T Current
            {
                get { return parent.Current; }
            }

            #endregion

            #region Члены IDisposable

            public void Dispose()
            {
                parent.Dispose();
            }

            #endregion

            #region Члены IEnumerator

            object System.Collections.IEnumerator.Current
            {
                get { return parent.Current; }
            }

            public bool MoveNext()
            {
                try
                {
                    var res = parent.MoveNext();
                    if (res)
                        index++;
                    return res;
                }
                catch
                {
                    parent = owner.GetEnumerator();
                    for (int i = 0; i < index && parent.MoveNext(); i++) ;
                    return MoveNext();
                }
            }

            public void Reset()
            {
                parent.Reset();
            }

            #endregion
        }

        private struct DoubleStringCacheItem
        {
            public double key;
            public string value;
        }

        private static readonly DoubleStringCacheItem[] cachedDoubleString = new DoubleStringCacheItem[8];
        private static int cachedDoubleStringsIndex = 0;
        private static string[] divFormats = {
                                                 ".#",
                                                 ".##",
                                                 ".###",
                                                 ".####",
                                                 ".#####",
                                                 ".######",
                                                 ".#######",
                                                 ".########",
                                                 ".#########",
                                                 ".##########",
                                                 ".###########",
                                                 ".############",
                                                 ".#############",
                                                 ".##############",
                                                 ".###############"
                                             };

        internal static string DoubleToString(double d)
        {
            if (d == 0.0)
                return "0";
            if (double.IsPositiveInfinity(d))
                return "Infinity";
            if (double.IsNegativeInfinity(d))
                return "-Infinity";
            if (double.IsNaN(d))
                return "NaN";
            for (var i = 8; i-- > 0; )
            {
                if (cachedDoubleString[i].key == d)
                    return cachedDoubleString[i].value;
            }
            //return dtoString(d);
            var abs = System.Math.Abs(d);
            string res = null;
            if (abs < 1.0)
            {
                if (d == (d % 0.000001))
                    return res = d.ToString("0.####e-0", System.Globalization.CultureInfo.InvariantCulture);
            }
            else if (abs >= 1e+21)
                return res = d.ToString("0.####e+0", System.Globalization.CultureInfo.InvariantCulture);
            int neg = (d < 0 || (d == -0.0 && double.IsNegativeInfinity(1.0 / d))) ? 1 : 0;
            if (d == 100000000000000000000d)
                res = "100000000000000000000";
            else if (d == -100000000000000000000d)
                res = "-100000000000000000000";
            else
                res = abs < 1.0 ? neg == 1 ? "-0" : "0" : ((d < 0 ? "-" : "") + ((ulong)(System.Math.Abs(d))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            abs %= 1.0;
            if (abs != 0 && res.Length < (15 + neg))
                res += abs.ToString(divFormats[15 - res.Length + neg], System.Globalization.CultureInfo.InvariantCulture);
            cachedDoubleString[cachedDoubleStringsIndex].key = d;
            cachedDoubleString[cachedDoubleStringsIndex++].value = res;
            cachedDoubleStringsIndex &= 7;
            return res;
        }

        private struct IntStringCacheItem
        {
            public int key;
            public string value;
        }

        private static readonly IntStringCacheItem[] intStringCache = new IntStringCacheItem[8]; // Обрати внимание на константы внизу
        private static int intStrCacheIndex = -1;

        public static string Int32ToString(int value)
        {
            for (var i = 8; i-- > 0; )
            {
                if (intStringCache[i].key == value)
                    return intStringCache[i].value;
            }
            return (intStringCache[intStrCacheIndex = (intStrCacheIndex + 1) & 7] = new IntStringCacheItem { key = value, value = value.ToString(CultureInfo.InvariantCulture) }).value;
        }

        public static bool ParseNumber(string code, out double value, int radix)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, radix, ParseNumberOptions.Default);
        }

        public static bool ParseNumber(string code, out double value, ParseNumberOptions options)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, 0, options);
        }

        public static bool ParseNumber(string code, out double value, int radix, ParseNumberOptions options)
        {
            int index = 0;
            return ParseNumber(code, ref index, out value, radix, options);
        }

        public static bool ParseNumber(string code, ref int index, out double value)
        {
            return ParseNumber(code, ref index, out value, 0, ParseNumberOptions.Default);
        }

        public static bool ParseNumber(string code, int index, out double value, ParseNumberOptions options)
        {
            return ParseNumber(code, ref index, out value, 0, options);
        }

        public static bool isDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        public static bool ParseNumber(string code, ref int index, out double value, int radix, ParseNumberOptions options)
        {
            bool raiseOctal = (options & ParseNumberOptions.RaiseIfOctal) != 0;
            bool processOctal = (options & ParseNumberOptions.ProcessOctal) != 0;
            bool allowAutoRadix = (options & ParseNumberOptions.AllowAutoRadix) != 0;
            bool allowFloat = (options & ParseNumberOptions.AllowFloat) != 0;
            if (code == null)
                throw new ArgumentNullException("code");
            value = double.NaN;
            if (radix != 0 && (radix < 2 || radix > 36))
                return false;
            if (code.Length == 0)
            {
                value = 0;
                return true;
            }
            int i = index;
            while (i < code.Length && char.IsWhiteSpace(code[i]) && !Tools.isLineTerminator(code[i]))
                i++;
            if (i >= code.Length)
            {
                value = 0.0;
                return true;
            }
            const string nan = "NaN";
            for (int j = i; ((j - i) < nan.Length) && (j < code.Length); j++)
            {
                if (code[j] != nan[j - i])
                    break;
                else if (j > i && code[j] == 'N')
                {
                    index = j + 1;
                    value = double.NaN;
                    return true;
                }
            }
            int sig = 1;
            if (code[i] == '-' || code[i] == '+')
                sig = 44 - code[i++];
            const string infinity = "Infinity";
            for (int j = i; ((j - i) < infinity.Length) && (j < code.Length); j++)
            {
                if (code[j] != infinity[j - i])
                    break;
                else if (code[j] == 'y')
                {
                    index = j + 1;
                    value = sig * double.PositiveInfinity;
                    return true;
                }
            }
            bool res = false;
            bool skiped = false;
            if (allowAutoRadix && i + 1 < code.Length)
            {
                while (code[i] == '0' && i + 1 < code.Length && code[i + 1] == '0')
                {
                    skiped = true;
                    i++;
                }
                if ((i + 1 < code.Length) && (code[i] == '0'))
                {
                    if ((radix == 0 || radix == 16)
                        && !skiped
                        && (code[i + 1] == 'x' || code[i + 1] == 'X'))
                    {
                        i += 2;
                        radix = 16;
                    }
                    else if (radix == 0 && isDigit(code[i + 1]))
                    {
                        if (raiseOctal)
                            throw new SyntaxError("Octal literals not allowed in strict mode");
                        i += 1;
                        if (processOctal)
                            radix = 8;
                        res = true;
                    }
                }
            }
            if (allowFloat && radix == 0)
            {
                ulong temp = 0;
                int scount = 0;
                int deg = 0;
                while (i < code.Length)
                {
                    if (!isDigit(code[i]))
                        break;
                    else
                    {
                        if (scount <= 18)
                        {
                            temp = temp * 10 + (ulong)(code[i++] - '0');
                            scount++;
                        }
                        else
                        {
                            deg++;
                            i++;
                        }
                        res = true;
                    }
                }
                if (!res && (i >= code.Length || code[i] != '.'))
                    return false;
                if (i < code.Length && code[i] == '.')
                {
                    i++;
                    while (i < code.Length)
                    {
                        if (!isDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 18)
                            {
                                temp = temp * 10 + (ulong)(code[i++] - '0');
                                scount++;
                                deg--;
                            }
                            else
                                i++;
                            res = true;
                        }
                    }
                }
                if (!res)
                    return false;
                if (i < code.Length && (code[i] == 'e' || code[i] == 'E'))
                {
                    i++;
                    int td = 0;
                    int esign = code[i] >= 43 && code[i] <= 45 ? 44 - code[i++] : 1;
                    scount = 0;
                    while (i < code.Length)
                    {
                        if (!isDigit(code[i]))
                            break;
                        else
                        {
                            if (scount <= 6)
                                td = td * 10 + (code[i++] - '0');
                            else
                                i++;
                        }
                    }
                    deg += td * esign;
                }
                if (deg != 0)
                {
                    if (deg < 0)
                    {
                        if (deg < -18)
                        {
                            value = temp * 1e-18;
                            deg += 18;
                        }
                        else
                        {
                            value = (double)((decimal)temp * (decimal)System.Math.Pow(10.0, deg));
                            deg = 0;
                        }
                    }
                    else
                    {
                        if (deg > 10)
                        {
                            value = (double)((decimal)temp * 10000000000M);
                            deg -= 10;
                        }
                        else
                        {
                            value = (double)((decimal)temp * (decimal)System.Math.Pow(10.0, deg));
                            deg = 0;
                        }
                    }
                    if (deg != 0)
                    {
                        if (deg == -324)
                        {
                            value *= 1e-323;
                            value *= 0.1;
                        }
                        else
                        {
                            var exp = System.Math.Pow(10.0, deg);
                            value *= exp;
                        }
                    }
                }
                else
                    value = temp;
                if (value == 0 && skiped && raiseOctal)
                    throw new SyntaxError("Octal literals not allowed in strict mode");
                value *= sig;
                index = i;
                return true;
            }
            else
            {
                if (radix == 0)
                    radix = 10;
                value = 0;
                bool extended = false;
                double doubleTemp = 0.0;
                ulong temp = 0;
                while (i < code.Length)
                {
                    var sign = anum(code[i]);
                    if (sign >= radix || (NumChars[sign] != code[i] && (NumChars[sign] + ('a' - 'A')) != code[i]))
                    {
                        break;
                    }
                    else
                    {
                        if (extended)
                            doubleTemp = doubleTemp * radix + sign;
                        else
                        {
                            temp = temp * (uint)radix + (uint)sign;
                            if ((temp & 0xFE00000000000000) != 0)
                            {
                                extended = true;
                                doubleTemp = temp;
                            }
                        }
                        res = true;
                    }
                    i++;
                }
                if (!res)
                {
                    value = double.NaN;
                    return false;
                }
                value = extended ? doubleTemp : temp;
                value *= sig;
                index = i;
                return true;
            }
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        public static string Unescape(string code)
        {
            return Unescape(code, false);
        }

        public static string Unescape(string code, bool processUnknown)
        {
            if (code == null)
                throw new ArgumentNullException("code");
            StringBuilder res = null;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == '\\' && i + 1 < code.Length)
                {
                    if (res == null)
                    {
                        res = new StringBuilder(code.Length);
                        for (var j = 0; j < i; j++)
                            res.Append(code[j]);
                    }
                    i++;
                    switch (code[i])
                    {
                        case 'x':
                        case 'u':
                            {
                                if (i + (code[i] == 'u' ? 5 : 3) > code.Length)
                                {
                                    throw new SyntaxError("Invalid escape code (\"" + code + "\")");
                                }
                                string c = code.Substring(i + 1, code[i] == 'u' ? 4 : 2);
                                ushort chc = 0;
                                if (ushort.TryParse(c, System.Globalization.NumberStyles.HexNumber, null, out chc))
                                {
                                    char ch = (char)chc;
                                    res.Append(ch);
                                    i += c.Length;
                                }
                                else
                                {
                                    throw new SyntaxError("Invalid escape sequence '\\" + code[i] + c + "'");
                                }
                                break;
                            }
                        case 't':
                            {
                                res.Append('\t');
                                break;
                            }
                        case 'f':
                            {
                                res.Append('\f');
                                break;
                            }
                        case 'v':
                            {
                                res.Append('\v');
                                break;
                            }
                        case 'b':
                            {
                                res.Append('\b');
                                break;
                            }
                        case 'n':
                            {
                                res.Append('\n');
                                break;
                            }
                        case 'r':
                            {
                                res.Append('\r');
                                break;
                            }
                        default:
                            {
                                if (isDigit(code[i]))
                                {
                                    var ccode = code[i] - '0';
                                    if (i + 1 < code.Length && isDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    if (i + 1 < code.Length && isDigit(code[i + 1]))
                                        ccode = ccode * 10 + (code[++i] - '0');
                                    res.Append((char)ccode);
                                }
                                else if (processUnknown)
                                    res.Append(code[i]);
                                else
                                {
                                    res.Append('\\');
                                    res.Append(code[i]);
                                }
                                break;
                            }
                    }
                }
                else if (res != null)
                    res.Append(code[i]);
            }
            return (res as object ?? code).ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool isLineTerminator(char c)
        {
            return (c == '\u000A') || (c == '\u000D') || (c == '\u2028') || (c == '\u2029');
        }

        internal static void skipComment(string code, ref int index, bool skipSpaces)
        {
            bool work;
            do
            {
                if (code.Length <= index)
                    return;
                work = false;
                if (code[index] == '/' && index + 1 < code.Length)
                {
                    switch (code[index + 1])
                    {
                        case '/':
                            {
                                index += 2;
                                while (index < code.Length && !Tools.isLineTerminator(code[index]))
                                    index++;
                                work = true;
                                break;
                            }
                        case '*':
                            {
                                index += 2;
                                while (index + 1 < code.Length && (code[index] != '*' || code[index + 1] != '/'))
                                    index++;
                                if (index + 1 >= code.Length)
                                    throw new SyntaxError("Unexpected end of source.");
                                index += 2;
                                work = true;
                                break;
                            }
                    }
                }
            } while (work);
            if (skipSpaces)
                while ((index < code.Length) && (char.IsWhiteSpace(code[index])))
                    index++;
        }

        internal static string RemoveComments(string code, int startPosition)
        {
            StringBuilder res = null;// new StringBuilder(code.Length);
            for (int i = startPosition; i < code.Length; )
            {
                while (i < code.Length && char.IsWhiteSpace(code[i]))
                {
                    if (res != null)
                        res.Append(code[i++]);
                    else
                        i++;
                }
                var s = i;
                skipComment(code, ref i, false);
                if (s != i && res == null)
                {
                    res = new StringBuilder(code.Length);
                    for (var j = 0; j < s; j++)
                        res.Append(code[j]);
                }
                for (; s < i; s++)
                {
                    if (char.IsWhiteSpace(code[s]))
                        res.Append(code[s]);
                    else
                        res.Append(' ');
                }
                if (i >= code.Length)
                    continue;

                if (//Parser.ValidateName(code, ref i, false) ||
                    //Parser.ValidateNumber(code, ref i) ||
                    //Parser.ValidateRegex(code, ref i, false) ||
                    Parser.ValidateString(code, ref i, false))
                {
                    if (res != null)
                        for (; s < i; s++)
                            res.Append(code[s]);
                }
                else if (res != null)
                    res.Append(code[i++]);
                else
                    i++;
            }
            return (res as object ?? code).ToString();
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static bool isHex(char p)
        {
            if (p < '0')
                return false;
            if (p > 'f')
                return false;
            var c = anum(p);
            return c >= 0 && c < 16;
        }

#if INLINE
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
#endif
        internal static int anum(char p)
        {
            return ((p % 'a' % 'A' + 10) % ('0' + 10));
        }

        public static int CompareWithMask(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) - ((int)(ValueType)y & (int)(ValueType)mask);
        }

        public static bool IsEqual(Enum x, Enum y, Enum mask)
        {
            return ((int)(ValueType)x & (int)(ValueType)mask) == ((int)(ValueType)y & (int)(ValueType)mask);
        }
    }
}
