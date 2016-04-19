using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Statements;

namespace NiL.C.CodeDom.Declarations
{
    internal class CFunction : Entity
    {
        private MethodBuilder method;

        public virtual CType ReturnType { get; protected set; }
        public virtual CodeBlock Body { get; protected set; }
        public virtual Argument[] Parameters { get; protected set; }

        internal CFunction(CType returnType, Argument[] parameters, string name)
            : base(null, name)
        {
            if (returnType == null)
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
        }

        internal static ParseResult Parse(State state, string code, ref int index)
        {
            string typeName;
            string name;

            int i = index;
            if (!Parser.ValidateName(code, ref index))
                return new ParseResult();
            typeName = Parser.CanonizeTypeName(code.Substring(i, index - i));
            Definition def = state.GetDefinition(typeName, false);
            // позволяет int по умолчанию
            if (!(def is CType))
            {
                index = i;
                def = state.GetDefinition("int");
            }

            var type = Parser.ParseType(state, (CType)def, code, ref index, out name);
            var prms = (type.Definition as CFunction).Parameters;

            while (code.Length > index && char.IsWhiteSpace(code[index])) index++;
            i = index;
            if (Parser.ValidateName(code, index))
            {
                for (var j = 0; j < prms.Length; j++)
                {
                    if (prms[j].Type != EmbeddedEntities.Declarations[""])
                        throw new SyntaxError();

                    prms[j].Type = null;
                }

                while (Parser.ValidateName(code, ref index))
                {
                    string entityName;
                    var t = Parser.ParseType(state, (CType)state.GetDefinition(code.Substring(i, index - i)), code, ref index, out entityName);

                    for (var j = 0; j < prms.Length; j++)
                    {
                        if (prms[j].Name == entityName)
                        {
                            if (prms[j].Type != null)
                                throw new SyntaxError();

                            prms[j].Type = t;
                            break;
                        }
                    }

                    while (code.Length > index && char.IsWhiteSpace(code[index])) index++;
                    if (code[index] == ';')
                        index++;

                    while (code.Length > index && char.IsWhiteSpace(code[index])) index++;
                    i = index;
                }

                for (var j = 0; j < prms.Length; j++)
                {
                    if (prms[j].Type == null)
                        throw new SyntaxError();
                }
            }

            using (state.Scope)
            {
                Definition funcd;
                CFunction func;
                if (state.Definitions.TryGetValue(name, out funcd))
                {
                    func = (CFunction)funcd;
                    if (func.Body != null)
                        throw new SyntaxError("Try to redefine \"" + name + "\"");
                    if (prms.Length != func.Parameters.Length || func.ReturnType.Name != typeName)
                        throw new SyntaxError("Invalid signature for \"" + name + "\"");
                    for (var j = 0; j < prms.Length; j++)
                    {
                        if (prms[i].Type != func.Parameters[i].Type)
                            throw new SyntaxError("Invalid signature for \"" + name + "\"");
                    }
                    func.Parameters = prms;
                }
                else
                    state.Definitions[name] = func = (CFunction)type.Definition;

                while (char.IsWhiteSpace(code[index])) index++;
                if (code[index] == ';')
                    return new ParseResult() { IsParsed = true, Statement = func };
                var body = Statements.CodeBlock.Parse(state, code, ref index);
                if (!body.IsParsed)
                    throw new SyntaxError("Invalid function body at " + CodeCoordinates.FromTextPosition(code, index, 0));

                func.Body = (CodeBlock)body.Statement;
                return new ParseResult() { IsParsed = true, Statement = func };
            }
        }

        internal override void Bind(ModuleBuilder module)
        {
            if (method != null)
            {
                if (method.Module != module)
                    throw new InvalidOperationException(method + " associated with another module");
                return;
            }
            method = ((ModuleBuilder)module).DefineGlobalMethod(Name,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
                (Type)ReturnType.GetInfo(module),
                Parameters.Select(x => (Type)x.Type.GetInfo(module)).ToArray());
            for (var i = 0; i < Parameters.Length; i++)
                Parameters[i].Bind(method);
            //method.DefineParameter(i, ParameterAttributes.In, Parameters[i].Name);
        }

        internal override void Emit(ModuleBuilder module)
        {
            if (method == null)
                Bind(module);
            else if (method.Module != module)
                throw new InvalidOperationException(method + " associated with another module");
            else if (IsComplete)
                throw new InvalidOperationException(method + " already defined");
            if (Body.Lines.Length != 0)
                Body.Emit(method);
            if (Body.Lines.Length == 0 || !(Body.Lines[Body.Lines.Length - 1] is Return))
            {
                if (ReturnType.GetInfo(module) != typeof(void))
                {
                    var size = Marshal.SizeOf(ReturnType.GetInfo(module) as Type);
                    if (size % 4 != 0)
                        throw new InvalidOperationException();
                    size /= 4;
                    while (size-- > 0)
                        method.GetILGenerator().Emit(OpCodes.Ldc_I4_0);
                }
                method.GetILGenerator().Emit(OpCodes.Ret); // return в С не обязателен во всех ветках
            }
            IsComplete = true;
        }

        internal override System.Reflection.MemberInfo GetInfo(Module module)
        {
            if (method == null)
                throw new InvalidOperationException(method + " has not been declared");
            else if (method.Module != module)
                throw new InvalidOperationException(method + " associated with another module");
            return method;
        }

        internal override void Bind(MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        internal override void Emit(EmitMode mode, MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            if (Type == null)
            {
                var typeName = ReturnType.Name + (Parameters.Length > 0 ? "(" + Parameters.Aggregate("", (x, y) => x + ", " + y.Type.Name).Substring(2) + ")" : "(void)");
                Definition def = null;
                if (state.Definitions.TryGetValue(typeName, out def))
                    Type = (CType)def;
                else
                    state.Definitions[typeName] = Type = new CType(typeName) { Definition = this };
            }

            using (state.Scope)
            {
                for (var i = 0; i < Parameters.Length; i++)
                {
                    var p = Parameters[i];
                    p.Build(ref p, state);
                    Parameters[i] = p;
                }

                if (Body != null) // эта функция может быть описана typedef'ом
                {
                    var b = Body;
                    Body.Build(ref b, state);
                    if (Body != b)
                        throw new InvalidOperationException("Something wrong");
                }

                return false;
            }
        }

        public override string ToString()
        {
            return ReturnType.Name + " " + Name + (Parameters.Length > 0 ? "(" + Parameters.Aggregate("", (x, y) => x + ", " + y.Type.Name).Substring(2) + ")" : "(void)");
        }
    }
}
