using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Statements;

namespace NiL.C.CodeDom.Declarations
{
    internal class CFunction : Entity
    {
        private string returnTypeName;
        private MethodBuilder method;

        public virtual CType ReturnType { get; protected set; }
        public virtual CodeBlock Body { get; protected set; }
        public virtual Argument[] Parameters { get; protected set; }

        internal CFunction(CType returnType, Argument[] parameters, string name)
            : base(new CType(returnType.Name + (parameters.Length > 0 ? "(" + parameters.Aggregate("", (x, y) => x + ", " + y.Type.Name).Substring(2) + ")" : "(void)")), name)
        {
            if (returnType == null)
                throw new ArgumentNullException();
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException();
            ReturnType = returnType;
            Name = name;
            Parameters = parameters;
            base.Type.Declaration = this;
        }

        internal static ParseResult Parse(State state, string code, ref int index)
        {
            string typeName;
            string name;
            int i = index;

            if (!Parser.ValidateType(code, ref index))
                return new ParseResult();
            typeName = Parser.CanonizeTypeName(code.Substring(i, index - i));
            while (char.IsWhiteSpace(code[index]))
                index++;
            i = index;
            if (!Parser.ValidateName(true, code, ref index))
                return new ParseResult();
            name = code.Substring(i + (code[i] == '(' ? 1 : 0), index - i - (code[i] == '(' ? 1 : 0));
            if (string.IsNullOrEmpty(name))
                throw new SyntaxError("Invalid symbol name at " + CodeCoordinates.FromTextPosition(code, i, 0));
            if (state.Declarations.ContainsKey(name))
                throw new SyntaxError("Try to rediclarate \"" + name + "\" at " + CodeCoordinates.FromTextPosition(code, i, index - i));
            while (char.IsWhiteSpace(code[index])) index++;


            var prms = parseParameters(state, code, ref index);
            if (prms == null)
                return new ParseResult();

            Definition funcd;
            CFunction func;
            if (state.Declarations.TryGetValue(name, out funcd))
            {
                func = (CFunction)funcd;
                if (func.Body != null)
                    throw new SyntaxError("Try to redefine \"" + name + "\"");
                if (prms.Length != func.Parameters.Length || func.ReturnType != (CType)state.GetDeclaration(typeName))
                    throw new SyntaxError("Invalid signature for \"" + name + "\"");
                for (var j = 0; j < prms.Length; j++)
                {
                    if (prms[i].Type != func.Parameters[i].Type)
                        throw new SyntaxError("Invalid signature for \"" + name + "\"");
                }
                func.Parameters = prms;
            }
            else
                state.Declarations[name] = func = new CFunction((CType)state.GetDeclaration(typeName), prms, name);

            while (char.IsWhiteSpace(code[index])) index++;
            if (code[index] == ';')
                return new ParseResult() { IsParsed = true, Statement = new CFunction((CType)state.GetDeclaration(typeName), prms, name) };
            var body = Statements.CodeBlock.Parse(state, code, ref index);
            if (!body.IsParsed)
                throw new SyntaxError("Invalid function body at " + CodeCoordinates.FromTextPosition(code, index, 0));

            func.Body = (CodeBlock)body.Statement;
            return new ParseResult() { IsParsed = true, Statement = func };

        }

        private static Argument[] parseParameters(State state, string code, ref int index)
        {
            if (code[index] != '(')
                return null;
            do index++; while (char.IsWhiteSpace(code[index]));
            if (code[index] == ')' || Parser.Validate(code, "void", ref index))
            {
                if (code[index] != ')') // тут уже можно кидаться ошибками
                    throw new SyntaxError("Expected ')' at " + CodeCoordinates.FromTextPosition(code, index, 0));
                index++;
                return new Argument[0];
            }
            var prms = new List<Argument>();
            while (code[index] != ')')
            {
                if (prms.Count > 0)
                {
                    if (code[index] != ',')
                        throw new SyntaxError("Expected ',' at " + CodeCoordinates.FromTextPosition(code, index, 0));
                    do index++; while (char.IsWhiteSpace(code[index]));
                }
                var i = index;
                var typeName = "int";
                if (Parser.ValidateType(code, ref index))
                    typeName = Parser.CanonizeTypeName(code.Substring(i, index - i));
                while (char.IsWhiteSpace(code[index])) index++;
                i = index;
                if (!Parser.ValidateName(true, code, ref index))
                    throw new SyntaxError("Invalid name at " + CodeCoordinates.FromTextPosition(code, index, 0));
                string name = code.Substring(i, index - i);
                prms.Add(new Argument((CType)state.GetDeclaration(typeName), name, prms.Count));
                state.DeclareSymbol(prms[prms.Count - 1]);
                while (char.IsWhiteSpace(code[index])) index++;
            }
            index++;
            return prms.ToArray();
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
            if (Body.Lines.Length == 0
                || !(Body.Lines[Body.Lines.Length - 1] is Return))
                method.GetILGenerator().Emit(OpCodes.Ret); // return в С не обязателен во всех ветках
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

        protected override bool PreBuild(ref CodeNode self, State state)
        {
            var b = Body;
            Body.Prepare(ref b, state);
            if (Body != b)
                throw new InvalidOperationException("Something wrong");
            return false;
        }
    }
}
