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
        Postfix = 0x110,
        Prefix = 0x200,
        Unary2 = 0x310,
        Multiplicative = 0x410,
        Additive = 0x510,
        BitwiseShift = 0x610,
        Relational = 0x710,
        Equality = 0x810,
        BitwiseAnd = 0x910,
        BitwiseXor = 0xa10,
        BitwiseOr = 0xb10,
        LogicalAnd = 0xc10,
        LogicalOr = 0xd10,
        Condition = 0xe00,
        Assignment = 0xf00,
        Comma = 0x1010,
        Special = 0xff00
    }

#if !PORTABLE
    [Serializable]
#endif
    internal enum OperationType : int
    {
        None = 0,

        Get = 0x11,
        Push,

        Index = OperationTypeGroups.Postfix + 1,
        Call = OperationTypeGroups.Postfix + 2,
        GetMember = OperationTypeGroups.Postfix + 3,
        IndirectGetMember = OperationTypeGroups.Postfix + 4,
        PostIncriment = OperationTypeGroups.Postfix + 5,
        PostDecriment = OperationTypeGroups.Postfix + 6,

        PreIncriment = OperationTypeGroups.Prefix + 1,
        PreDecriment,
        Casr,
        SizeOf,
        GetPointer,
        Indirection,
        Positive,
        Negation,
        BitwiseNegation,
        LogicalNegation,

        Multiplicate = OperationTypeGroups.Multiplicative + 1,
        Division,
        Module,

        Addition = OperationTypeGroups.Additive + 1,
        Substraction,

        ShiftLeft = OperationTypeGroups.BitwiseShift + 1,
        ShiftRight,

        Less = OperationTypeGroups.Relational + 1,
        More = OperationTypeGroups.Relational + 2,
        LessOrEqual = OperationTypeGroups.Relational + 3,
        MoreOrEqual = OperationTypeGroups.Relational + 4,

        Equal = OperationTypeGroups.Equality + 1,
        NotEqual = OperationTypeGroups.Equality + 2,

        BitwiseAnd = OperationTypeGroups.BitwiseAnd + 1,

        BitwiseXor = OperationTypeGroups.BitwiseXor + 1,

        BitwiseOr = OperationTypeGroups.BitwiseOr + 1,

        LogicalAnd = OperationTypeGroups.LogicalAnd + 1,

        LogicalOr = OperationTypeGroups.LogicalOr + 1,

        Condition = OperationTypeGroups.Condition + 1,

        Assign = OperationTypeGroups.Assignment + 1,
        MulAssign = OperationTypeGroups.Assignment + 2,
        DivAssign = OperationTypeGroups.Assignment + 3,
        ModAssign = OperationTypeGroups.Assignment + 4,
        AddAssign = OperationTypeGroups.Assignment + 5,
        SubAssign = OperationTypeGroups.Assignment + 6,
        ShlAssign = OperationTypeGroups.Assignment + 7,
        ShrAssign = OperationTypeGroups.Assignment + 8,
        AndAssign = OperationTypeGroups.Assignment + 9,
        XorAssign = OperationTypeGroups.Assignment + 10,
        OrAssign = OperationTypeGroups.Assignment + 11,

        Comma = OperationTypeGroups.Comma + 1,

        BreackOpen = OperationTypeGroups.Special + 1
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

        protected override bool Build(ref CodeNode self, State state)
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
                    case OperationType.Get:
                        {
                            operationsStack.Peek().Parameter = new EntityAccessExpression(state.GetDefinition(operationsStack.Peek().Parameter.ToString()));
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
                    case OperationType.Multiplicate:
                    case OperationType.Addition:
                        {
                            var type = operationsStack.Peek().Type;
                            var operation = operationsStack.Pop();
                            var second = operationsStack.Pop();
                            var first = operationsStack.Pop();
                            switch (type)
                            {
                                case OperationType.Addition:
                                    operation.Parameter = new Addition((Expression)first.Parameter, (Expression)second.Parameter);
                                    break;
                                case OperationType.Multiplicate:
                                    operation.Parameter = new Multiplicate((Expression)first.Parameter, (Expression)second.Parameter);
                                    break;
                            }
                            operationsStack.Push(operation);
                            arn--;
                            break;
                        }
                    case OperationType.PreIncriment:
                    case OperationType.PostIncriment:
                        {
                            var operation = operationsStack.Pop();
                            operation.Parameter = new Increment((Expression)operationsStack.Pop().Parameter, (Increment.Type)operation.Parameter);
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

        public override string ToString()
        {
            return "<unbuild expression>";
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
                        while (code[index] == value[value.Length - 1] && Parser.ValidateString(code, ref index))
                        {
                            pindex++;
                            value = value.Substring(0, value.Length - 1) + code.Substring(pindex, index - pindex);
                            while (char.IsWhiteSpace(code[index])) index++;
                        }
                        if (value[value.Length - 1] == '\'' && ((wide && value.Length != 4) || (!wide && value.Length != 3)))
                            throw new SyntaxError("Invalid char constant at " + CodeCoordinates.FromTextPosition(code, pindex, value.Length));
                    }
                    popIfNeed(operationsStack, operations, OperationType.Push);
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
                        popIfNeed(operationsStack, operations, OperationType.Get);
                        operationsStack.Push(new Operation(OperationType.Get, name));
                    }
                    else
                    {
                        while (refDepth-- > 0)
                            operationsStack.Push(new Operation(OperationType.Indirection, null));
                        popIfNeed(operationsStack, operations, OperationType.Get);
                        operationsStack.Push(new Operation(OperationType.Get, name));
                        unary = false;
                    }

                    while (char.IsWhiteSpace(code[index])) index++;
                }
                switch (code[index])
                {
                    case '(':
                        {
                            if (operationsStack.Peek().Type == OperationType.Get) // вызов
                            {
                                //if (!unary)
                                //    throw new InvalidOperationException("something wrong");
                                operationsStack.Push(new Operation(OperationType.Call, null));
                            }
                            else // просто скобочки
                            {
                                throw new NotImplementedException();
                            }
                            operationsStack.Push(new Operation(OperationType.BreackOpen, null));
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case ')':
                        {
                            while (operationsStack.Peek().Type != OperationType.BreackOpen)
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
                                    operationsStack.Push(new Operation(OperationType.PostIncriment, Increment.Type.Postincriment));
                                    operationsStack.Push(t);
                                }
                                else
                                {
                                    operationsStack.Push(new Operation(OperationType.PreIncriment, Increment.Type.Preincriment));
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
                                    popIfNeed(operationsStack, operations, OperationType.Addition);
                                    operationsStack.Push(new Operation(OperationType.Addition, null));
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
                                popIfNeed(operationsStack, operations, OperationType.Multiplicate);
                                operationsStack.Push(new Operation(OperationType.Multiplicate, null));
                                unary = true;
                            }
                            do index++; while (char.IsWhiteSpace(code[index]));
                            break;
                        }
                    case ',':
                        {
                            if (!processComma)
                                goto case ';';
                            popIfNeed(operationsStack, operations, OperationType.Comma);
                            unary = true;
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

        private static void popIfNeed(Stack<Operation> operationsStack, List<Operation> operations, OperationType operation)
        {
            if (operationsStack.Count == 0)
                return;
            var stackGroup = (int)operationsStack.Peek().Type & (int)OperationTypeGroups.Special;
            var currentGroup = (int)operation & (int)OperationTypeGroups.Special;
            var leftHand = ((int)operation & 0xf0) != 0;
            while ((stackGroup < currentGroup || (leftHand && stackGroup == currentGroup)))
            {
                operations.Add(operationsStack.Pop());
                if (operationsStack.Count == 0)
                    return;
                stackGroup = (int)operationsStack.Peek().Type & (int)OperationTypeGroups.Special;
            }
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            first.Build(ref first, state);
            second.Build(ref second, state);
            return false;
        }
    }
}