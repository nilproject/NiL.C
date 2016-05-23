using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Declarations;
using NiL.C.CodeDom.Expressions;
using NiL.C.CodeDom.Statements;
using NiL.C.CodeDom;

namespace NiL.C
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
                        owner.Definitions.Remove(symbols[i]);
                    }
            }

            #endregion
        }

        internal readonly Dictionary<string, Definition> Definitions;
        internal Dictionary<string, string> Macros;

        private _Scope scope;
        public IDisposable Scope { get { return scope = new _Scope(this); } }

        internal void DefineMacros(string name, string value)
        {
            if (Macros == null)
                Macros = new Dictionary<string, string>();

            Macros[name] = value;
        }

        public State()
        {
            Definitions = new Dictionary<string, Definition>();
        }

        public Definition GetDefinition(string name)
        {
            return GetDefinition(name, true);
        }

        public Definition GetDefinition(string name, bool @throw)
        {
            Definition res;
            if (!Definitions.TryGetValue(name, out res)
                && !EmbeddedEntities.Declarations.TryGetValue(name, out res))
            {
                if (@throw)
                    throw new ArgumentException("Symbol \"" + name + "\" has not been defined");

                return null;
            }
            return res; // будет брошено исключение если чё
        }

        public void DeclareSymbol(Definition decl)
        {
            if (scope == null)
                throw new InvalidOperationException("Invalid scope");
            if (scope.symbols == null)
                scope.symbols = new List<string>();
            scope.symbols.Add(decl.Name);
            Definitions.Add(decl.Name, decl);
        }
    }

    internal static class Parser
    {
        internal delegate CodeNode ParseDelegate(State state, string code, ref int index);
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

            public _Rule(ValidateDelegate validateDelegate, ParseDelegate parseDel)
            {
                this.Validate = validateDelegate;
                this.Parse = parseDel;
            }
        }

        private static _Rule[][] rules = new _Rule[][]
        {
            new _Rule[] // 0
            {
                new _Rule((code, index) => ValidateName(code, index) || Validate(code, "void", index), CFunction.Parse),
                new _Rule(ValidateName, VariableDefinition.Parse),
            },
            new _Rule[] // 1
            {
                new _Rule("return", Return.Parse),
                new _Rule("for", For.Parse),
                new _Rule("while", While.Parse),
                new _Rule("do", DoWhile.Parse),
                new _Rule(ValidateName, VariableDefinition.Parse),
                new _Rule(ValidateName, Expression.Parse),
                new _Rule(ValidateOperator, Expression.Parse)
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
            var i = index;
            bool br = false;
            if (allowBrackets && code[i] == '(')
            {
                do i++; while (char.IsWhiteSpace(code[i]));
                br = true;
            }
            if (char.IsWhiteSpace(code[i]))
                return false;
            while (code[i] == '*' || char.IsWhiteSpace(code[index]))
                i++;
            if (!ValidateName(code, ref i, true))
                return false;
            if (allowBrackets && br)
            {
                while (char.IsWhiteSpace(code[i]))
                    i++;
                if (code[i] != ')')
                    return false;
            }

            index = i;
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
            if (code[index] == ',' || code[index] == ';')
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
                    if (pr != null)
                    {
                        if ((code.Length <= newIndex || code[newIndex] != ';') && code[newIndex - 1] != '}')
                            throw new SyntaxError("Expected ';'");

                        index = newIndex + 1;
                        return pr;
                    }
                }
            }

            var cord = CodeCoordinates.FromTextPosition(code, sindex, 0);
            throw new SyntaxError("Unexpected token at " + cord + " : "
                + code.Substring(index, System.Math.Min(20, code.Length - index)).Split(new[] { ' ', '\n', '\r' })[0]);
        }

        internal static CType ParseType(State state, CType rootType, string code, ref int index, out string entityName)
        {
            entityName = null;
            List<object> modifiers = new List<object>();
            Stack<object> stack = new Stack<object>();
            bool work = true;
            bool rightHand = false;
            var i = index;
            int openedBreaks = 0;
            object aster = '*';

            while (work)
            {
                switch (code[i])
                {
                    case '*':
                        {
                            if (rightHand)
                                throw new SyntaxError("");
                            stack.Push(aster);
                            break;
                        }
                    case '(':
                        {
                            if (rightHand)
                            {
                                stack.Push(ParseParameters(state, code, ref i));
                            }
                            else
                            {
                                openedBreaks++;
                                stack.Push(null); // вот такой маркер открытой скобки
                            }
                            break;
                        }
                    case '[':
                        {
                            if (rightHand)
                            {
                                // TODO
                                throw new NotImplementedException();
                            }
                            else
                                throw new SyntaxError("Unexpected token");
                        }
                    case ')':
                        {
                            rightHand = true;
                            if (stack.Count == 0 || openedBreaks == 0)
                            {
                                work = false;
                                i--;
                                break;
                            }

                            while (stack.Peek() != null)
                                modifiers.Add(stack.Pop());

                            stack.Pop();
                            openedBreaks--;
                            break;
                        }
                    default:
                        {
                            if (!char.IsWhiteSpace(code[i]))
                            {
                                if (rightHand)
                                {
                                    work = false;
                                    do i--; while (char.IsWhiteSpace(code[i]));
                                }
                                else
                                {
                                    var s = i;
                                    if (ValidateName(code, ref i))
                                    {
                                        rightHand = true;
                                        entityName = code.Substring(s, i - s);
                                        i--;
                                    }
                                    else throw new SyntaxError("");
                                }
                            }
                            break;
                        }
                }
                i++;
            }

            if (openedBreaks != 0)
                throw new SyntaxError("Expected ')'");

            while (stack.Count > 0)
                modifiers.Add(stack.Pop());

            for (var m = modifiers.Count; m-- > 0;)
            {
                if (modifiers[m] == aster)
                    rootType = rootType.MakePointerType();
                else if (modifiers[m] is Parameter[])
                {
                    var func = new CFunction(rootType, modifiers[m] as Parameter[], entityName);
                    func.Build(ref func, state);
                    rootType = func.Type;
                }
                else
                    throw new NotImplementedException();
            }
            index = i;

            return rootType;
        }

        internal static Parameter[] ParseParameters(State state, string code, ref int index)
        {
            if (code[index] != '(')
                return null;
            do index++; while (char.IsWhiteSpace(code[index]));
            if (code[index] == ')' || Parser.Validate(code, "void", ref index))
            {
                if (code[index] != ')') // тут уже можно кидаться ошибками
                    throw new SyntaxError("Expected ')' at " + CodeCoordinates.FromTextPosition(code, index, 0));
                index++;
                return new Parameter[0];
            }
            var prms = new List<Parameter>();
            bool explicitType = false;
            while (code[index] != ')')
            {
                if (prms.Count > 0)
                {
                    if (code[index] != ',')
                        throw new SyntaxError("Expected ',' at " + CodeCoordinates.FromTextPosition(code, index, 0));
                    do index++; while (char.IsWhiteSpace(code[index]));
                }
                var i = index;
                string name;
                CType type;
                if (Parser.ValidateName(code, ref index))
                {
                    name = Parser.CanonizeTypeName(code.Substring(i, index - i));
                    type = state.GetDefinition(name, false) as CType;
                    if (type != null)
                    {
                        if (prms.Count > 0 && !explicitType)
                            throw new SyntaxError("All arguments must have a some kind of type specification (all implicit or all explicit)");

                        type = ParseType(state, type, code, ref index, out name);
                        explicitType = true;
                    }
                    else
                    {
                        if (prms.Count > 0 && explicitType)
                            throw new SyntaxError("All arguments must have a some kind of type specification (all implicit or all explicit)");

                        type = state.GetDefinition("") as CType;
                    }
                }
                else throw new SyntaxError("Invalid name at " + CodeCoordinates.FromTextPosition(code, index, 0));
                while (char.IsWhiteSpace(code[index])) index++;
                prms.Add(new Parameter(type, name, prms.Count + 1));
                while (char.IsWhiteSpace(code[index])) index++;
            }
            index++;
            return prms.ToArray();
        }
    }
}
