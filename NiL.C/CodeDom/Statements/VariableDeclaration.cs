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
        private string[] varNames;

        public string TypeName { get; private set; }
        public Entity[] Variables { get; private set; }
        public CodeNode[] Initializators { get; private set; }

        internal static ParseResult Parse(State state, string code, ref int index)
        {
            int pindex = index;
            if (!Parser.ValidateTypeName(code, ref index))
                return new ParseResult();
            string typename = code.Substring(pindex, index - pindex);
            while (char.IsWhiteSpace(code[index]))
                index++;
            pindex = index;
            if (!Parser.ValidateName(true, code, ref index))
                return new ParseResult();
            index = pindex;
            //var type = (CType)state.GetDeclaration(typename);
            var variables = new List<string>();
            List<CodeNode> initializators = null;
            while (code[index] != ';')
            {
                if (variables.Count > 0 && code[index] == ',')
                    do index++; while (char.IsWhiteSpace(code[index]));
                pindex = index;
                if (!Parser.ValidateName(true, code, ref index))
                    throw new SyntaxError("Expected identifier at " + CodeCoordinates.FromTextPosition(code, pindex, 0));
                int refDepth = 0;
                var br = code[pindex] == '(';
                if (br)
                    do pindex++; while (char.IsWhiteSpace(code[index]));
                if (code[pindex] == '*')
                {
                    while (code[pindex] == '*' || char.IsWhiteSpace(code[pindex]))
                    {
                        if (code[pindex] == '*')
                            refDepth++;
                        pindex++;
                    }
                }
                var name = code.Substring(pindex, index - pindex - (br ? 1 : 0)).Trim();
                while (char.IsWhiteSpace(code[index]))
                    index++;

                //var vtype = type;
                //while (refDepth-- > 0)
                //    vtype = vtype.MakePointerType();
                //var symbol = new Variable(vtype, name);

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

                //variables.Add(symbol);
                variables.Add(name);

                if (code[index] != ',' && code[index] != ';')
                    throw new SyntaxError("Unexpected token at " + CodeCoordinates.FromTextPosition(code, index, 0));
            }
            return new ParseResult
            {
                IsParsed = true,
                Statement = new VariableDefinition
                {
                    TypeName = typename,
                    varNames = variables.ToArray(),
                    Initializators = initializators != null ? initializators.ToArray() : new Expression[variables.Count]
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
                if (Initializators[i] != null)
                {
                    var text = Initializators[i].ToString();
                    res.Append(text, 1, text.Length - 2);
                }
                else
                    res.Append(Variables[i].Type.Name, baseType.Name.Length, Variables[i].Type.Name.Length - baseType.Name.Length)
                        .Append(Variables[i].Name);
            }
            return res.ToString();
        }

        protected override bool Prepare(ref CodeNode self, State state)
        {
            return false;
        }

        internal override void Emit(System.Reflection.Emit.MethodBuilder method)
        {
            for (var i = 0; i < Variables.Length; i++)
                Variables[i].Bind(method);
            for (var i = 0; i < Initializators.Length; i++)
                if (Initializators[i] != null)
                    Initializators[i].Emit(EmitMode.SetOrNone, method);
        }
    }
}
