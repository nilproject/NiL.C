using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Declarations;

namespace NiL.C.CodeDom
{
    internal enum SymbolType
    {
        Type,
        Field
    }


    internal sealed class State
    {
        private sealed class _Scope : IDisposable
        {
            private State owner;
            private _Scope oldScope;
            public List<string> symbols;

            public _Scope(State state)
            {
                this.owner = state;
                oldScope = owner.scope;
            }

            #region Члены IDisposable

            public void Dispose()
            {
                owner.scope = oldScope;
                if (symbols != null)
                    for (var i = 0; i < symbols.Count; i++)
                    {
                        owner.Declarations.Remove(symbols[i]);
                    }
            }

            #endregion
        }

        public readonly Dictionary<string, Definition> Declarations;

        private _Scope scope;
        public IDisposable Scope { get { return scope = new _Scope(this); } }

        public State()
        {
            Declarations = new Dictionary<string, Definition>();
        }

        public Definition GetDeclaration(string name)
        {
            Definition res;
            if (!Declarations.TryGetValue(name, out res)
                && !EmbeddedEntities.Declarations.TryGetValue(name, out res))
                throw new ArgumentException("Symbol \"" + name + "\" has not been defined");
            return res; // будет брошено исключение если чё
        }

        public void DeclareSymbol(Definition decl)
        {
            if (scope == null)
                throw new InvalidOperationException("Invalid scope");
            if (scope.symbols == null)
                scope.symbols = new List<string>();
            scope.symbols.Add(decl.Name);
            Declarations.Add(decl.Name, decl);
        }
    }

    internal struct ParseResult
    {
        public bool IsParsed;
        public CodeNode Statement;
    }

    internal static class Parser
    {
        internal delegate ParseResult ParseDelegate(State state, string code, ref int index);
        internal delegate bool ValidateDelegate(string code, int index);

        private class _Rule
        {
            public ValidateDelegate Validate;
            public ParseDelegate Parse;

            public _Rule(string token, ParseDelegate parseDel)
            {
                this.Validate = (string code, int pos) => Parser.Validate(code, token, pos);
                this.Parse = parseDel;
            }

            public _Rule(ValidateDelegate valDel, ParseDelegate parseDel)
            {
                this.Validate = valDel;
                this.Parse = parseDel;
            }
        }

        private static _Rule[][] rules = new _Rule[][]
        {
            new _Rule[] // 0
            {
                new _Rule(ValidateType, Declarations.CFunction.Parse)
            },
            new _Rule[] // 1
            {
                new _Rule(ValidateType, Statements.VariableDefinition.Parse),
                new _Rule(ValidateName, Expressions.Expression.Parse),
                new _Rule(ValidateOperator, Expressions.Expression.Parse),
                new _Rule("return", Statements.Return.Parse)
            }
        };

        private static bool ValidateOperator(string code, int index)
        {
            return isOperator(code[index]);
        }

        internal static bool Validate(string code, string patern, int index)
        {
            return Validate(code, patern, ref index);
        }

        internal static bool Validate(string code, string patern, ref int index)
        {
            int i = 0;
            int j = index;
            bool needInc = false;
            while (i < patern.Length)
            {
                if (j >= code.Length)
                    return false;
                while (char.IsWhiteSpace(patern[i])
                    && code.Length > j
                    && char.IsWhiteSpace(code[j]))
                {
                    j++;
                    needInc = true;
                }
                if (needInc)
                {
                    i++;
                    if (i == patern.Length)
                        break;
                    if (code.Length <= j)
                        return false;
                    needInc = false;
                }
                if (code[j] != patern[i])
                    return false;
                i++;
                j++;
            }
            index = j;
            return true;
        }

        internal static bool ValidateTypeName(string code, ref int index)
        {
            if (Validate(code, "void", ref index)
                || Validate(code, "char", ref index)
                || Validate(code, "auto", ref index)
                || Validate(code, "int", ref index))
                return isIdentificatorTerminator(code[index]);
            if (Validate(code, "signed", ref index))
            {
                return (Validate(code, " char", ref index)
                || Validate(code, " int", ref index)
                || Validate(code, " short int", ref index)
                || Validate(code, " short", ref index)
                || Validate(code, " long long int", ref index)
                || Validate(code, " long long", ref index)
                || Validate(code, " long int", ref index)
                || Validate(code, " long", ref index)
                || true) && isIdentificatorTerminator(code[index]);
            }
            if (Validate(code, "unsigned", ref index))
            {
                return (Validate(code, " char", ref index)
                || Validate(code, " int", ref index)
                || Validate(code, " short int", ref index)
                || Validate(code, " short", ref index)
                || Validate(code, " long long int", ref index)
                || Validate(code, " long long", ref index)
                || Validate(code, " long int", ref index)
                || Validate(code, " long", ref index)
                || true) && isIdentificatorTerminator(code[index]);
            }
            if (Validate(code, "long", ref index))
            {
                return (Validate(code, " int", ref index)
                || Validate(code, " long int", ref index)
                || Validate(code, " long", ref index)
                || Validate(code, " double _Complex", ref index)
                || Validate(code, " double", ref index)
                || true) && isIdentificatorTerminator(code[index]);
            }
            if (Validate(code, "short", ref index))
            {
                return (Validate(code, " int", ref index)
                || true) && isIdentificatorTerminator(code[index]);
            }
            if (Validate(code, "float", ref index)
                && (Validate(code, " _Complex", ref index) || true))
                return isIdentificatorTerminator(code[index]);
            if (Validate(code, "double", ref index)
                && (Validate(code, " _Complex", ref index) || true))
                return isIdentificatorTerminator(code[index]);
            if (Validate(code, "struct ", ref index))
            {
                if (ValidateName(code, ref index, true))
                    return isIdentificatorTerminator(code[index]);
                index -= "struct ".Length;
                return false;
            }
            return ValidateName(code, ref index) && isIdentificatorTerminator(code[index]);
        }


        internal static bool ValidateType(string code, int index)
        {
            return ValidateType(code, ref index);
        }

        internal static bool ValidateType(string code, ref int index)
        {
            if (!ValidateTypeName(code, ref index))
                return false;
            while (char.IsWhiteSpace(code[index]) || code[index] == '*')
                index++;
            //do index--; while (char.IsWhiteSpace(code[index]));
            return true;
        }

        private static readonly char[] separators = new char[] { ' ', '\u000A', '\u000D', '\u2028', '\u2029' };
        internal static string CanonizeTypeName(string name)
        {
            if (name.IndexOfAny(separators) == -1)
                return name;
            var t = name.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (t.Length == 1)
                return t[0];
            var res = new StringBuilder(t[0]);
            for (var i = 1; i < t.Length; i++)
                res.Append(' ').Append(t[i]);
            return res.Replace(" *", " ").ToString();
        }

        internal static ValidateDelegate ValidateName(bool allowBrackets)
        {
            return (code, index) => ValidateName(allowBrackets, code, ref index);
        }

        internal static bool ValidateName(bool allowBrackets, string code, ref int index)
        {
            bool br = false;
            if (allowBrackets && code[index] == '(')
            {
                do index++; while (char.IsWhiteSpace(code[index]));
                br = true;
            }
            if (char.IsWhiteSpace(code[index]))
                return false;
            while (code[index] == '*' || char.IsWhiteSpace(code[index]))
                index++;
            if (!ValidateName(code, ref index, true))
                return false;
            if (allowBrackets && br)
            {
                while (char.IsWhiteSpace(code[index]))
                    index++;
                if (code[index] != ')')
                    return false;
            }
            return true;
        }

        internal static bool ValidateName(string code, int index)
        {
            return ValidateName(code, ref index, true);
        }

        internal static bool ValidateName(string code, ref int index)
        {
            return ValidateName(code, ref index, true);
        }

        internal static bool ValidateName(string name, int index, bool reserveControl)
        {
            return ValidateName(name, ref index, reserveControl);
        }

        internal static bool ValidateName(string code, ref int index, bool reserveControl)
        {
            int j = index;
            if ((code[j] != '_') && (!char.IsLetter(code[j])))
                return false;
            j++;
            while (j < code.Length)
            {
                if ((code[j] != '_') && (!char.IsLetterOrDigit(code[j])))
                    break;
                j++;
            }
            if (index == j)
                return false;
            string name = reserveControl ? code.Substring(index, j - index) : null;
            if (reserveControl)
                switch (name)
                {
                    case "break":
                    case "case":
                    case "catch":
                    case "continue":
                    case "delete":
                    case "default":
                    case "do":
                    case "else":
                    //case "finally":
                    case "for":
                    case "function":
                    case "if":
                    //case "in":
                    //case "instanceof":
                    case "is":
                    //case "new":
                    case "return":
                    case "switch":
                    case "this":
                    case "throw":
                    case "try":
                    case "typedef":
                    case "typeof":
                    //case "var":
                    case "void":
                    case "while":
                    case "with":
                    case "true":
                    case "false":
                    case "null":
                    case "export":
                    case "extends":
                    //case "import":
                    //case "super":
                    //case "class":
                    case "const":
                    //case "debugger":
                    case "enum":
                    case "union":
                    case "struct":
                        //case "yield":
                        return false;
                }
            index = j;
            return true;
        }

        internal static bool ValidateNumber(string code, ref int index)
        {
            double fictive = 0.0;
            return Tools.ParseNumber(code, ref index, out fictive, 0, Tools.ParseNumberOptions.AllowFloat | Tools.ParseNumberOptions.AllowAutoRadix);
        }

        internal static bool ValidateString(string code, ref int index)
        {
            return ValidateString(code, ref index, false);
        }

        internal static bool ValidateString(string code, ref int index, bool @throw)
        {
            int j = index;
            if (code[j] == 'L') // wide
                j++;
            if (j + 1 < code.Length && ((code[j] == '\'') || (code[j] == '"')))
            {
                char fchar = code[j];
                j++;
                while (code[j] != fchar)
                {
                    if (code[j] == '\\')
                    {
                        j++;
                        if ((code[j] == '\r') && (code[j + 1] == '\n'))
                            j++;
                        else if ((code[j] == '\n') && (code[j + 1] == '\r'))
                            j++;
                    }
                    else if (Tools.isLineTerminator(code[j]) || (j + 1 >= code.Length))
                    {
                        if (!@throw)
                            return false;
                        throw new SyntaxError("Unterminated string constant");
                    }
                    j++;
                    if (j >= code.Length)
                        return false;
                }
                index = ++j;
                return true;
            }
            return false;
        }

        internal static bool ValidateValue(string code, int index)
        {
            return ValidateValue(code, ref index);
        }

        internal static bool ValidateValue(string code, ref int index)
        {
            int j = index;
            if (ValidateString(code, ref index, false))
                return true;
            if ((code.Length - j >= 4) && (code[j] == 'n' || code[j] == 't' || code[j] == 'f'))
            {
                string codeSs4 = code.Substring(j, 4);
                if ((codeSs4 == "null") || (codeSs4 == "true") || ((code.Length >= 5) && (codeSs4 == "fals") && (code[j + 4] == 'e')))
                {
                    index += codeSs4 == "fals" ? 5 : 4;
                    return true;
                }
            }
            return ValidateNumber(code, ref index);
        }

        public static bool isOperator(char c)
        {
            return (c == '+')
                || (c == '-')
                || (c == '*')
                || (c == '/')
                || (c == '%')
                || (c == '^')
                || (c == '&')
                || (c == '!')
                || (c == '<')
                || (c == '>')
                || (c == '=')
                || (c == '?')
                || (c == ':')
                || (c == ',')
                || (c == '.');
        }

        internal static bool isIdentificatorTerminator(char c)
        {
            return c == ' '
                || Tools.isLineTerminator(c)
                || isOperator(c)
                || char.IsWhiteSpace(c)
                || (c == '{')
                || (c == '\v')
                || (c == '}')
                || (c == '(')
                || (c == ')')
                || (c == ';')
                || (c == '[')
                || (c == ']')
                || (c == '\'')
                || (c == '"');
        }

        internal static CodeNode Parse(State state, string code, ref int index, int ruleset)
        {
            while ((index < code.Length) && (char.IsWhiteSpace(code[index])))
                index++;
            if (index >= code.Length || code[index] == '}')
                return null;
            int sindex = index;
            if (code[index] == ','
                || code[index] == ';')
            {
                index++;
                return null;
            }
            if (index >= code.Length)
                return null;
            for (int i = 0; i < rules[ruleset].Length; i++)
            {
                if (rules[ruleset][i].Validate(code, index))
                {
                    var newIndex = index;
                    var pr = rules[ruleset][i].Parse(state, code, ref newIndex);
                    if (pr.IsParsed)
                    {
                        index = newIndex;
                        return pr.Statement;
                    }
                }
            }
            var cord = CodeCoordinates.FromTextPosition(code, sindex, 0);
            throw new SyntaxError("Unexpected token at " + cord + " : "
                + code.Substring(index, System.Math.Min(20, code.Length - index)).Split(new[] { ' ', '\n', '\r' })[0]);
        }
    }
}
