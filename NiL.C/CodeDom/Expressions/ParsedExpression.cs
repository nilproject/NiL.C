using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NiL.C.CodeDom.Expressions
{

#if !PORTABLE
    [Serializable]
#endif
    internal sealed class ParsedExpression : Expression
    {
        private List<Operation> operations;

        public ParsedExpression(List<Operation> operations)
        {
            this.operations = operations;
        }

        protected override bool Build(ref CodeNode self, State state)
        {
            var operationsStack = new Stack<Operation>();

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
                            break;
                        }
                    case OperationType.Get:
                        {
                            operationsStack.Peek().Parameter = new EntityAccessExpression(state.GetDefinition(operationsStack.Peek().Parameter.ToString()));
                            break;
                        }
                    case OperationType.Call:
                        {
                            var operation = operationsStack.Pop();

                            if (operationsStack.Peek().Type != OperationType.Get)
                                throw new InvalidOperationException();

                            var function = (Expression)operationsStack.Pop().Parameter;

                            var args = new Expression[(int)operation.Parameter];
                            for (var j = args.Length; j-- > 0;)
                                args[j] = (Expression)operationsStack.Pop().Parameter;

                            operation.Parameter = new Call(function, args);
                            operationsStack.Push(operation);
                            break;
                        }
                    case OperationType.Less:
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

                                case OperationType.Less:
                                    operation.Parameter = new Less((Expression)first.Parameter, (Expression)second.Parameter);
                                    break;
                            }
                            operationsStack.Push(operation);
                            break;
                        }
                    case OperationType.PreIncriment:
                    case OperationType.PostIncriment:
                        {
                            var operation = operationsStack.Pop();
                            operation.Parameter = new Increment((Expression)operationsStack.Peek().Parameter, (Increment.Type)operation.Parameter);
                            operationsStack.Pop();
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

            self = (Expression)operationsStack.Pop().Parameter;
            return true;
        }

        public override string ToString()
        {
            return "<not builded expression>";
        }
    }
}
