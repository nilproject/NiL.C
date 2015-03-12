using System;
using System.Collections.Generic;
using System.Text;
using NiL.C.CodeDom.Declarations;


namespace NiL.C.CodeDom.Expressions
{
#if !PORTABLE
    [Serializable]
#endif
    internal enum OperationTypeGroups : int
    {
        None = 0x0,
        Assign = 0x10,
        Choice = 0x20,
        LOr = 0x30,
        LAnd = 0x40,
        Or = 0x50,
        Xor = 0x60,
        And = 0x70,
        Logic1 = 0x80,
        Logic2 = 0x90,
        Bit = 0xa0,
        Arithmetic0 = 0xb0,
        Arithmetic1 = 0xc0,
        Unary0 = 0xd0,
        Unary1 = 0xe0,
        Special = 0xF0
    }

#if !PORTABLE
    [Serializable]
#endif
    internal enum OperationType : int
    {
        None = OperationTypeGroups.None + 0,
        Assign = OperationTypeGroups.Assign + 0,
        Ternary = OperationTypeGroups.Choice + 0,

        LogicalOr = OperationTypeGroups.LOr,
        LogicalAnd = OperationTypeGroups.LAnd,
        Or = OperationTypeGroups.Or,
        Xor = OperationTypeGroups.Xor,
        And = OperationTypeGroups.And,

        Equal = OperationTypeGroups.Logic1 + 0,
        NotEqual = OperationTypeGroups.Logic1 + 1,
        StrictEqual = OperationTypeGroups.Logic1 + 2,
        StrictNotEqual = OperationTypeGroups.Logic1 + 3,

        InstanceOf = OperationTypeGroups.Logic2 + 0,
        In = OperationTypeGroups.Logic2 + 1,
        More = OperationTypeGroups.Logic2 + 2,
        Less = OperationTypeGroups.Logic2 + 3,
        MoreOrEqual = OperationTypeGroups.Logic2 + 4,
        LessOrEqual = OperationTypeGroups.Logic2 + 5,

        SignedShiftLeft = OperationTypeGroups.Bit + 0,
        SignedShiftRight = OperationTypeGroups.Bit + 1,
        UnsignedShiftRight = OperationTypeGroups.Bit + 2,

        Addition = OperationTypeGroups.Arithmetic0 + 0,
        Substract = OperationTypeGroups.Arithmetic0 + 1,
        Multiply = OperationTypeGroups.Arithmetic1 + 0,
        Module = OperationTypeGroups.Arithmetic1 + 1,
        Division = OperationTypeGroups.Arithmetic1 + 2,

        Negative = OperationTypeGroups.Unary0 + 0,
        Positive = OperationTypeGroups.Unary0 + 1,
        LogicalNot = OperationTypeGroups.Unary0 + 2,
        Not = OperationTypeGroups.Unary0 + 3,
        TypeOf = OperationTypeGroups.Unary0 + 4,
        Delete = OperationTypeGroups.Unary0 + 5,
        GetPointer = OperationTypeGroups.Unary0 + 6,

        Incriment = OperationTypeGroups.Unary1 + 0,
        Decriment = OperationTypeGroups.Unary1 + 1,

        Call = OperationTypeGroups.Special + 0,
        New = OperationTypeGroups.Special + 1,
        CallOrConvert = OperationTypeGroups.Special + 2,

        BrackOpen = OperationTypeGroups.Special + 3,
        SquareBrackOpen = OperationTypeGroups.Special + 4,
        SquareBrackClose = OperationTypeGroups.Special + 5,
        Push = OperationTypeGroups.Special + 6,
        GetVariable = OperationTypeGroups.Special + 7,
        Indirection = OperationTypeGroups.Special + 8
    }

    internal sealed class Operation
    {
        public OperationType Type;
        public object Parameter;

        public Operation(OperationType operationType, object value)
        {
            this.Type = operationType;
            this.Parameter = value;
        }

        public override string ToString()
        {
            return Type + " " + Parameter;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    internal sealed class ParsedExpression : Expression
    {
        private Stack<Operation> operationsStack;
        private List<Operation> operations;

        public ParsedExpression(Stack<Operation> operationsStack, List<Operation> operations)
        {
            this.operationsStack = operationsStack;
            this.operations = operations;
        }

        protected override bool PreBuild(ref CodeNode self, State state)
        {
            int arn = 0;
            while (operationsStack.Count != 0)
                operations.Add(operationsStack.Pop());
            for (var i = 0; i < operations.Count; i++)
            {
                operationsStack.Push(operations[i]);
                switch (operationsStack.Peek().Type)
                {
                    case OperationType.Push:
                        {
                            var prm = operationsStack.Peek().Parameter;
                            if (prm is string)
                            {
                                var value = prm.ToString();
                                double number = 0;
                                int index = 0;
                                if (Tools.ParseNumber(value, ref index, out number))
                                {
                                    if ((int)number == number)
                                    {
                                        operationsStack.Peek().Parameter = new Constant((int)number);
                                    }
                                    else if ((long)number == number)
                                    {
                                        operationsStack.Peek().Parameter = new Constant((long)number);
                                    }
                                    else
                                        operationsStack.Peek().Parameter = new Constant(number);
                                }
                                else
                                {
                                    if (value[value.Length - 1] == '\'')
                                    {
                                        if (value[0] == 'L')
                                            operationsStack.Peek().Parameter = new Constant((char)value[1]);
                                        else
                                            operationsStack.Peek().Parameter = new Constant((byte)value[1]);
                                    }
                                    else if (value[value.Length - 1] == '"')
                                    {
                                        if (value[0] == 'L')
                                            operationsStack.Peek().Parameter = new Constant(Tools.Unescape(value.Substring(2, value.Length - 3)));
                                        else
                                            operationsStack.Peek().Parameter = new Constant(Encoding.ASCII.GetBytes(Tools.Unescape(value.Substring(1, value.Length - 2))));
                                    }
                                    else
                                        throw new ArgumentException("Can not convert expression: " + value);
                                }
                            }
                            else
                                throw new ArgumentException("Can not convert expression: " + prm);
                            arn++;
                            break;
                        }
                    case OperationType.GetVariable:
                        {
                            operationsStack.Peek().Parameter = new EntityAccessExpression(state.GetDeclaration(operationsStack.Peek().Parameter.ToString()));
                            arn++;
                            break;
                        }
                    case OperationType.Call:
                        {
                            var operation = operationsStack.Pop();
                            var function = (Expression)operationsStack.Pop().Parameter;
                            arn--;
                            var args = new Expression[arn];
                            for (var j = 0; j < args.Length; j++)
                                args[j] = (Expression)operationsStack.Pop().Parameter;
                            operation.Parameter = new Call(function, args);
                            operationsStack.Push(operation);
                            arn = 1;
                            break;
                        }
                    case OperationType.Addition:
                        {
                            var operation = operationsStack.Pop();
                            var first = operationsStack.Pop();
                            var second = operationsStack.Pop();
                            operation.Parameter = new Addition((Expression)first.Parameter, (Expression)second.Parameter);
                            operationsStack.Push(operation);
                            arn--;
                            break;
                        }
                    case OperationType.Incriment:
                        {
                            var operation = operationsStack.Pop();
                            operation.Parameter = new Incriment((Expression)operationsStack.Pop().Parameter, (Incriment.Type)operation.Parameter);
                            operationsStack.Push(operation);
                            break;
                        }
                    case OperationType.GetPointer:
                        {
                            var operation = operationsStack.Pop();
                            operation.Parameter = new GetPointer((Expression)operationsStack.Pop().Parameter);
                            operationsStack.Push(operation);
                            break;
                        }
                    case OperationType.Indirection:
                        {
                            var operation = operationsStack.Pop();
                            operation.Parameter = new Indirection((Expression)operationsStack.Pop().Parameter);
                            operationsStack.Push(operation);
                            break;
                        }
                    default:
                        throw new NotImplementedException(operationsStack.Peek().Type.ToString());
                }
            }
            if (arn != 1)
                throw new InvalidOperationException("Something wrong");
            self = (Expression)operationsStack.Pop().Parameter;
            return true;
        }
    }

#if !PORTABLE
    [Serializable]
#endif
    internal abstract class Expression : CodeNode
    {
        protected internal Expression first;
        protected internal Expression second;

        public Expression FirstOperand { get { return first; } }
        public Expression SecondOperand { get { return second; } }

        public virtual CType ResultType { get { throw new NotImplementedException(); } }

        public virtual bool IsContextIndependent
        {
            get
            {
                return (first == null || first is Constant || (first is Expression && ((Expression)first).IsContextIndependent))
                    && (second == null || second is Constant || (second is Expression && ((Expression)second).IsContextIndependent));
            }
        }

        protected Expression()
        {

        }

        protected Expression(Expression first, Expression second)
        {
            this.first = first;
            this.second = second;
        }

        internal static ParseResult Parse(State state, string code, ref int index)
        {
            return Parse(state, code, ref index, true);
        }

        internal static ParseResult Parse(State state, string code, ref int index, bool processComma)
        {
            Stack<Operation> operationsStack = new Stack<Operation>();
            List<Operation> operations = new List<Operation>();
            bool unary = true;

            bool work = true;
            int pindex;
            while (work)
            {
                pindex = index;
                if (Parser.ValidateValue(code, ref index))
                {
                    var value = code.Substring(pindex, index - pindex);
                    while (char.IsWhiteSpace(code[index])) index++;
                    pindex = index;
                    if (value[value.Length - 1] == '\'' || value[value.Length - 1] == '"')
                    {
                        bool wide = value[0] == 'L';
                        while (code[index] == value[value.Length - 1]
                            && Parser.ValidateString(code, ref index))
                        {
                            pindex++;
                            value = value.Substring(0, value.Length - 1) + code.Substring(pindex, index - pindex);
                            while (char.IsWhiteSpace(code[index])) index++;
                        }
                        if (value[value.Length - 1] == '\'' && ((wide && value.Length != 4) || (!wide && value.Length != 3)))
                            throw new SyntaxError("Invalid char constant at " + CodeCoordinates.FromTextPosition(code, pindex, value.Length));
                    }
                    operationsStack.Push(new Operation(OperationType.Push, value));

                    unary = false;
                    while (char.IsWhiteSpace(code[index])) index++;
                }
                else if (Parser.ValidateName(true, code, ref index))
                {
                    int refDepth = 0;
                    var br = code[pindex] == '(';
                    if (br)
                        do pindex++; while (char.IsWhiteSpace(code[index]));
                    if (code[pindex] == '*')
                    {
                        while (code[pindex] == '*' || char.IsWhiteSpace(code[pindex]))
                        {
                            refDepth++;
                            pindex++;
                        }
                    }
                    var name = code.Substring(pindex, index - pindex - (br ? 1 : 0)).TrimEnd();

                    if (br && refDepth == 0)
                    {
                        if (refDepth > 0)
                            throw new InvalidOperationException();
                        operationsStack.Push(new Operation(OperationType.CallOrConvert, name));
                    }
                    else
                    {
                        while (refDepth-- > 0)
                            operationsStack.Push(new Operation(OperationType.Indirection, null));
                        operationsStack.Push(new Operation(OperationType.GetVariable, name));
                        unary = false;
                    }

                    while (char.IsWhiteSpace(code[index])) index++;
                }
                switch (code[index])
                {
                    case '(':
                        {
                            if (operationsStack.Peek().Type == OperationType.GetVariable) // вызов
                            {
                                //if (!unary)
                                //    throw new InvalidOperationException("something wrong");
                                operationsStack.Push(new Operation(OperationType.Call, null));
                            }
                            else // просто скобочки
                            {
                                throw new NotImplementedException();
                            }
                            operationsStack.Push(new Operation(OperationType.BrackOpen, null));
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case ')':
                        {
                            while (operationsStack.Peek().Type != OperationType.BrackOpen)
                                operations.Add(operationsStack.Pop());
                            operationsStack.Pop();
                            if (operationsStack.Count > 0 && operationsStack.Peek().Type == OperationType.Call)
                            {
                                var t = operationsStack.Pop(); // call
                                operations.Add(operationsStack.Pop());
                                operations.Add(t);
                            }
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case '+':
                        {
                            if (code[index + 1] == '+')
                            {
                                if (!unary) // operand already in stack
                                {
                                    var t = operationsStack.Pop();
                                    operationsStack.Push(new Operation(OperationType.Incriment, Incriment.Type.Postincriment));
                                    operationsStack.Push(t);
                                }
                                else
                                {
                                    operationsStack.Push(new Operation(OperationType.Incriment, Incriment.Type.Preincriment));
                                }
                                index++;
                            }
                            else
                            {
                                if (unary)
                                {
                                    // do nothing
                                }
                                else
                                {
                                    var t = operationsStack.Pop();
                                    operationsStack.Push(new Operation(OperationType.Addition, null));
                                    operationsStack.Push(t);
                                }
                                unary = true;
                            }
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case '&':
                        {
                            if (unary)
                            {
                                operationsStack.Push(new Operation(OperationType.GetPointer, null));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case '*':
                        {
                            if (unary)
                            {
                                operationsStack.Push(new Operation(OperationType.Indirection, null));
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case ',':
                        {
                            if (!processComma)
                                goto case ';';
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case ';':
                        {
                            work = false;
                            continue;
                        }
                    default:
                        throw new SyntaxError("Unknown operator " + code[index]);
                }
            }

            return new ParseResult()
            {
                IsParsed = true,
                Statement = new ParsedExpression(operationsStack, operations)
            };
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        protected override bool PreBuild(ref CodeNode self, State state)
        {
            throw new NotImplementedException();
        }
    }
}