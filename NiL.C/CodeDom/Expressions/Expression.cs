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
        Postfix = 0x100,
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

        Index = OperationTypeGroups.Postfix + 1,
        Call,
        GetMember,
        IndirectGetMember,
        PostIncriment,
        PostDecriment,
        Get,
        Push,

        PreIncriment = OperationTypeGroups.Prefix + 1,
        PreDecriment,
        Cast,
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

        BreacketOpen = OperationTypeGroups.Special + 1,
        SquareBreacketOpen,
        ArgumentPlaceholder
    }

    internal sealed class Operation
    {
        public OperationType Type;
        public object Parameter;

        public Operation(OperationType type, object value)
        {
            this.Type = type;
            this.Parameter = value;
        }

        public Operation(OperationType type)
        {
            Type = type;
        }

        public override string ToString()
        {
            return Type + " " + Parameter;
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

        internal static CodeNode Parse(State state, string code, ref int index)
        {
            return Parse(state, code, ref index, true);
        }

        internal static CodeNode Parse(State state, string code, ref int index, bool processComma)
        {
            Stack<Operation> operationsStack = new Stack<Operation>();
            List<Operation> operations = new List<Operation>();
            var brksOpnd = 0;
            var unary = true;
            var work = true;
            while (work)
            {
                if (Parser.Validate(code, "sizeof", ref index))
                {
                    operationsStack.Push(new Operation(OperationType.SizeOf, null));
                    while (char.IsWhiteSpace(code[index])) index++;
                }

                parseValue(code, ref index, operationsStack, operations, ref unary);

                switch (code[index])
                {
                    case '(':
                        {
                            if (operationsStack.Count > 0 && operationsStack.Peek().Type == OperationType.Get)
                            {
                                operationsStack.Push(new Operation(OperationType.Call, null));
                                operationsStack.Push(new Operation(OperationType.BreacketOpen, null));
                                brksOpnd++;
                            }
                            else if (unary)
                            {
                                var start = index;
                                do index++; while (char.IsWhiteSpace(code[index]));
                                var nstart = index;
                                CType type = null;

                                if (Parser.ValidateName(code, ref index)
                                    && (type = state.GetDefinition(code.Substring(nstart, index - nstart), false) as CType) != null)
                                {
                                    string fake;
                                    type = Parser.ParseType(state, type, code, ref index, out fake);

                                    var isSizeOf = operationsStack.Count > 0 && operationsStack.Peek().Type == OperationType.SizeOf;
                                    operationsStack.Push(new Operation(OperationType.Cast, type));
                                    if (isSizeOf)
                                        operationsStack.Push(new Operation(OperationType.Push, 0));
                                }
                                else
                                {
                                    operationsStack.Push(new Operation(OperationType.BreacketOpen, null));
                                    brksOpnd++;
                                }
                            }
                            else
                            {
                                operationsStack.Push(new Operation(OperationType.BreacketOpen, null));
                                brksOpnd++;
                            }

                            break;
                        }
                    case ')':
                        {
                            if (operationsStack.Count == 0)
                                goto case ';';

                            int itemsCount = 0;
                            int prmsCount = 0;

                            while (operationsStack.Count > 0
                                && operationsStack.Peek().Type != OperationType.BreacketOpen)
                            {
                                operations.Add(operationsStack.Pop());
                                itemsCount++;
                                if ((prmsCount == 0)
                                 || (operations[operations.Count - 1].Type == OperationType.ArgumentPlaceholder))
                                    prmsCount++;
                            }

                            if (operationsStack.Count == 0)
                            {
                                while (itemsCount-- > 0)
                                {
                                    operationsStack.Push(operations[operations.Count - 1]);
                                    operations.RemoveAt(operations.Count - 1);
                                }

                                goto case ';';
                            }

                            operationsStack.Pop();

                            if (operationsStack.Count > 0 && operationsStack.Peek().Type == OperationType.Call)
                            {
                                var t = operationsStack.Pop();
                                t.Parameter = prmsCount;
                                operations.Add(operationsStack.Pop());
                                operations.Add(t);
                                operationsStack.Push(new Operation(OperationType.ArgumentPlaceholder));
                            }

                            brksOpnd--;

                            break;
                        }
                    case '[':
                        {
                            if (operationsStack.Peek().Type == OperationType.Get)
                            {
                                operationsStack.Push(new Operation(OperationType.Index, null));
                                operationsStack.Push(new Operation(OperationType.SquareBreacketOpen, null));
                            }
                            else
                            {
                                throw new SyntaxError(); // Надо разобрать случай "0[a]"
                            }

                            brksOpnd++;

                            break;
                        }
                    case ']':
                        {
                            int itemsCount = 0;
                            int prmsCount = 0;

                            while (operationsStack.Count > 0
                                && operationsStack.Peek().Type != OperationType.SquareBreacketOpen)
                            {
                                operations.Add(operationsStack.Pop());
                                itemsCount++;
                                if ((prmsCount == 0) || (operations[operations.Count - 1].Type == OperationType.ArgumentPlaceholder))
                                    prmsCount++;
                            }

                            if (operationsStack.Count == 0)
                            {
                                while (itemsCount-- > 0)
                                {
                                    operationsStack.Push(operations[operations.Count - 1]);
                                    operations.RemoveAt(operations.Count - 1);
                                }

                                goto case ';';
                            }

                            for (var i = prmsCount; i-- > 1;)
                            {
                                operations.Add(new Operation(OperationType.None));
                            }

                            operationsStack.Pop();
                            brksOpnd--;

                            var indexOperation = operationsStack.Pop();
                            indexOperation.Parameter = prmsCount;
                            var indexArgument = operationsStack.Pop();
                            operationsStack.Push(indexOperation);
                            operationsStack.Push(indexArgument);
                            while (prmsCount-- > 0)
                            {
                                operationsStack.Push(operations[operations.Count - 1]);
                                operations.RemoveAt(operations.Count - 1);
                            }

                            break;
                        }
                    case '=':
                        {
                            popIfNeed(operationsStack, operations, OperationType.Assign);
                            operationsStack.Push(new Operation(OperationType.Assign, null));
                            unary = true;

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

                            break;
                        }
                    case '<':
                        {
                            if (code[index + 1] == '<')
                            {
                                var t = operationsStack.Pop();
                                operationsStack.Push(new Operation(OperationType.ShiftLeft, null));
                                operationsStack.Push(t);
                                index++;
                            }
                            else
                            {
                                if (unary)
                                {
                                    throw new SyntaxError();
                                }
                                else
                                {
                                    popIfNeed(operationsStack, operations, OperationType.Less);
                                    operationsStack.Push(new Operation(OperationType.Less, null));
                                }

                                unary = true;
                            }

                            break;
                        }
                    case ',':
                        {
                            if (!processComma && brksOpnd == 0)
                                goto case ';';

                            while (popIfNeed(operationsStack, operations, OperationType.Comma)) ;

                            operationsStack.Push(new Operation(OperationType.ArgumentPlaceholder));

                            unary = true;

                            break;
                        }
                    case '}':
                    case ';':
                    default:
                        {
                            work = false;
                            continue;
                        }
                }

                do index++; while (char.IsWhiteSpace(code[index]));
            }

            while (operationsStack.Count != 0)
            {
                if (operationsStack.Peek().Type == OperationType.BreacketOpen)
                    throw new SyntaxError();

                operations.Add(operationsStack.Pop());
            }

            operations.RemoveAll(x => x.Type == OperationType.ArgumentPlaceholder);

            return new ParsedExpression(operations);
        }

        private static void parseValue(string code, ref int index, Stack<Operation> operationsStack, List<Operation> operations, ref bool unary)
        {
            var pindex = index;
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
            else if (Parser.ValidateName(false, code, ref index))
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
                var name = code.Substring(pindex, index - pindex).TrimEnd();

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
        }

        private static bool popIfNeed(Stack<Operation> operationsStack, List<Operation> operations, OperationType operation)
        {
            if (operationsStack.Count == 0)
                return false;

            var result = false;

            var stackGroup = (int)operationsStack.Peek().Type & (int)OperationTypeGroups.Special;
            var currentGroup = (int)operation & (int)OperationTypeGroups.Special;
            var leftHand = ((int)operation & 0xf0) != 0;
            while ((stackGroup < currentGroup || (leftHand && stackGroup == currentGroup)))
            {
                result = true;
                operations.Add(operationsStack.Pop());
                if (operationsStack.Count == 0)
                    return result;

                stackGroup = (int)operationsStack.Peek().Type & (int)OperationTypeGroups.Special;
            }

            return result;
        }

        internal override void Emit(EmitMode mode, System.Reflection.Emit.MethodBuilder method)
        {
            throw new NotImplementedException();
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            first?.Build(ref first, state);
            second?.Build(ref second, state);
            return false;
        }
    }
}