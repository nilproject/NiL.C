using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NiL.C.CodeDom.Declarations;
using NiL.C.CodeDom.Expressions;

namespace NiL.C.CodeDom.Statements
{
    internal sealed class VariableDefinition : Statement
    {
        public Entity[] Variables { get; private set; }
        public CodeNode[] Initializators { get; private set; }

        internal static ParseResult Parse(State state, string code, ref int index)
        {
            int pindex = index;
            if (!Parser.ValidateTypeName(code, ref index))
                return new ParseResult();
            string typename = Parser.CanonizeTypeName(code.Substring(pindex, index - pindex));
            var rootType = state.GetDefinition(typename, false);
            if (!(rootType is CType))
                return new ParseResult();

            var variables = new List<Entity>();
            List<CodeNode> initializators = null;
            while (code[index] != ';')
            {
                if (code[index] == ',')
                {
                    if (variables.Count > 0)
                        do index++; while (char.IsWhiteSpace(code[index]));
                    else
                        throw new SyntaxError("Unexpected char at " + CodeCoordinates.FromTextPosition(code, pindex, 0));
                }
                pindex = index;

                string name;
                var type = Parser.ParseType(state, (CType)rootType, code, ref index, out name);
                while (char.IsWhiteSpace(code[index]))
                    index++;

                if (code[index] == '=')
                {
                    do index++; while (char.IsWhiteSpace(code[index]));
                    var initializator = Expression.Parse(state, code, ref index, false);
                    if (!initializator.IsParsed)
                        throw new SyntaxError("Unexpected expression at " + CodeCoordinates.FromTextPosition(code, index, 0));
                    if (initializators == null)
                    {
                        initializators = new List<CodeNode>(variables.Count + 1);
                        while (initializators.Count < variables.Count)
                            initializators.Add(null);
                    }
                    initializators.Add((Expression)initializator.Statement);
                    while (char.IsWhiteSpace(code[index]))
                        index++;
                }
                else if (initializators != null)
                    initializators.Add(null);

                variables.Add(new Variable(type, name));

                if (code[index] != ',' && code[index] != ';')
                    throw new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(code, index, 0));
            }
            return new ParseResult
            {
                IsParsed = true,
                Statement = new VariableDefinition
                {
                    Variables = variables.ToArray(),
                    Initializators = initializators != null ? initializators.ToArray() : null
                }
            };
        }

        public override string ToString()
        {
            var baseType = Variables[0].Type;
            while (baseType.IsPointer)
                baseType = baseType.TargetType;
            StringBuilder res = new StringBuilder(baseType.Name).Append(' ');
            for (var i = 0; i < Variables.Length; i++)
            {
                if (i > 0)
                    res.Append(", ");

                baseType = Variables[i].Type;
                while (baseType.IsPointer)
                {
                    baseType = baseType.TargetType;
                    res.Append('*');
                }
                res.Append(Variables[i].Name);

                if (Initializators[i] != null)
                {
                    res.Append(" = ");
                    res.Append(Initializators[i]);
                }
            }
            return res.Append(';').ToString();
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            for (var i = 0; i < Variables.Length; i++)
                state.DeclareSymbol(Variables[i]);
            if (Initializators != null)
                for (var i = 0; i < Initializators.Length; i++)
                {
                    Initializators[i].Build(ref Initializators[i], state);
                }
            return false;
        }

        internal override void Emit(System.Reflection.Emit.MethodBuilder method)
        {
            for (var i = 0; i < Variables.Length; i++)
                Variables[i].Bind(method);
            if (Initializators != null)
                for (var i = 0; i < Initializators.Length; i++)
                    if (Initializators[i] != null)
                    {
                        if (Initializators[i] is IWantToGetType)
                            (Initializators[i] as IWantToGetType).SetType(Variables[i].Type.GetInfo(method.Module) as Type);
                        Initializators[i].Emit(EmitMode.Get, method);
                        Variables[i].Emit(EmitMode.SetOrNone, method);
                    }
        }
    }
}
