// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ISB.Runtime
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed class Instruction
    {
        public enum OperandKind
        {
            None = 0,
            Label,
            Identifier,
            Integer,
            Number,
            String,
            Boolean
        }

        // Does nothing.
        public const string NOP = "nop";

        // Unconditional jump.
        //   br <label>
        public const string BR = "br";

        // Conditional jump.
        //   br_if <label1> <label2>
        //
        //   (1) value := Stack.Pop()
        //   (2) if value JumpTo(label1) else JumpTo(label2)
        public const string BR_IF = "br_if";

        // Sets a value to a register.
        //   set <register_no>
        //
        //   (1) value := Stack.Pop()
        //   (2) register[register_no] := value  ; <register_no> can be 0, 1, 2, 3, ...
        public const string SET = "set";

        // Gets a value from a register.
        //   get <register_no>
        //
        //   (1) Stack.Push(register[register_no])  ; <register_no> can be 0, 1, 2, 3, ...
        public const string GET = "get";

        // Stores a value to a variable.
        //   store <variable>
        //
        //   (1) value := Stack.Pop()
        //   (2) variable := value
        public const string STORE = "store";

        // Pushes the value of a variable into the stack.
        //   load <variable>
        //
        //   (1) Stack.Push(variable)
        public const string LOAD = "load";

        // Stores a value to an indexable location of an array.
        //   store_arr <array> <dimension>
        //
        //   (1) arrayItemRef := array
        //   (2) for i in [0, dimension):
        //   (3)   index := Stack.Pop()
        //   (4)   arrayItemRef := arrayItemRef[index]
        //   (5) value := Stack.Pop()
        //   (6) *arrayItemRef := value
        public const string STORE_ARR = "store_arr";

        // Loads a value from an indexable location of an array.
        //   load_arr <array> <dimension>
        //
        //   (1) arrayItemRef := array
        //   (2) for i in [0, dimension):
        //   (3)   index := Stack.Pop()
        //   (4)   arrayItemRef := arrayItemRef[index]
        //   (5) Stack.Push(*arrayItemRef)
        public const string LOAD_ARR = "load_arr";

        // Pushes a number value into the stack.
        //   push <number>
        //
        //   (1) Stack.Push(number)
        public const string PUSH = "push";

        // Pushes a string value into the stack.
        //   push <str>
        //
        //   (1) Stack.Push(str)
        public const string PUSHS = "pushs";

        // Pushes a boolean value into the stack.
        //   push <bool>
        //
        //   (1) Stack.Push(bool)
        public const string PUSHB = "pushb";

        // Calls a function.
        // It's the caller's duty to push arguments into the stack.
        // It's the callee's duty to use ret <n> to clean the stack frame.
        // It's the callee's duty to push the return value into the stack.
        //     push | pushs <argument(n-1)>
        //     push | pushs <argument(n-2)>
        //     ...
        //     push | pushs <argument(0)>
        //     call <func>
        //
        //   (1) Stack.Push(IP)
        //   (2) IP := <func.Entry>  ; JumpTo(func.Entry)
        public const string CALL = "call";

        // Returns from a function.
        // It's the caller's duty to push arguments into the stack.
        // It's the callee's duty to use ret <n> to clean the stack frame.
        // It's the callee's duty to push the return value into the stack.
        //   ret <n>
        //
        //   (1) return_add := Stack.Pop()
        //   (2) for i in [0, n):
        //   (3)   argument(i) := Stack.Pop()
        //   (4) Run(func.Body)
        //   (5) Stack.Push(return_value)
        //   (6) IP := <return_add>  ; JumpTo(return_add)
        public const string RET = "ret";

        // Calls a function in an external lib.
        // It's the caller's duty to push arguments into the stack.
        // It's the runtime engine's duty to pop out arguments from the stack.
        // It's the runtime engine's duty to push the return value into the stack.
        //     push | pushs <argument(n-1)>
        //     push | pushs <argument(n-2)>
        //     ...
        //     push | pushs <argument(0)>
        //     call_lib <lib> <func>
        //
        //   (1) Call(lib.func)
        //   (2)   return_add := Stack.Pop()
        //   (3)   for i in [0, n):
        //   (4)     argument(i) := Stack.Pop()
        //   (5)   Run(func.Body)
        //   (6)   Stack.Push(return_value)
        public const string CALL_LIB = "call_lib";

        // Stores a value to a lib's writable property.
        //   store_lib <lib> <property>
        //
        //   (1) value := Stack.Pop()
        //   (2) lib.property := value
        public const string STORE_LIB = "store_lib";

        // Loads a value from a lib's property.
        //   load_lib <lib> <property>
        //
        //   (1) Stack.Push(lib.property)
        public const string LOAD_LIB = "load_lib";

        // Adds two operands.
        //   add
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 + value2
        //   (4) Stack.Push(result)
        public const string ADD = "add";

        // Substracts two operands.
        //   sub
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 - value2
        //   (4) Stack.Push(result)
        public const string SUB = "sub";

        // Times two operands.
        //   mul
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 * value2
        //   (4) Stack.Push(result)
        public const string MUL = "mul";

        // Divides an operand by another.
        //   div
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 / value2
        //   (4) Stack.Push(result)
        public const string DIV = "div";

        // Modulo. Divides an operand by another and returns the reminder.
        //   mod
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 % value2
        //   (4) Stack.Push(result)
        public const string MOD = "mod";

        // Determines if two values are equal.
        //   eq
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 == value2
        //   (4) Stack.Push(result)
        public const string EQ = "eq";

        // Determines if two values are not equal.
        //   ne
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 != value2
        //   (4) Stack.Push(result)
        public const string NE = "ne";

        // Compares two values.
        //   lt
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 < value2
        //   (4) Stack.Push(result)
        public const string LT = "lt";

        // Compares two values.
        //   gt
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 > value2
        //   (4) Stack.Push(result)
        public const string GT = "gt";

        // Compares two values.
        //   le
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 <= value2
        //   (4) Stack.Push(result)
        public const string LE = "le";

        // Compares two values.
        //   ge
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 >= value2
        //   (4) Stack.Push(result)
        public const string GE = "ge";

        public const string ZeroLiteral = "0";
        public const string TrueLiteral = "True";
        public const string FalseLiteral = "False";

        private static readonly Dictionary<string, (OperandKind, OperandKind)> instructionSet =
            new Dictionary<string, (OperandKind, OperandKind)>()
        {
            { NOP, (OperandKind.None, OperandKind.None) },
            { BR, (OperandKind.Label, OperandKind.None) },
            { BR_IF, (OperandKind.Label, OperandKind.Label) },
            { SET, (OperandKind.Integer, OperandKind.None) },
            { GET, (OperandKind.Integer, OperandKind.None) },
            { STORE, (OperandKind.Identifier, OperandKind.None) },
            { LOAD, (OperandKind.Identifier, OperandKind.None) },
            { STORE_ARR, (OperandKind.Identifier, OperandKind.Integer) },
            { LOAD_ARR, (OperandKind.Identifier, OperandKind.Integer) },
            { PUSH, (OperandKind.Number, OperandKind.None) },
            { PUSHS, (OperandKind.String, OperandKind.None) },
            { PUSHB, (OperandKind.Boolean, OperandKind.None) },
            { CALL, (OperandKind.Label, OperandKind.None) },
            { RET, (OperandKind.Integer, OperandKind.None) },
            { CALL_LIB, (OperandKind.Label, OperandKind.Label) },
            { STORE_LIB, (OperandKind.Label, OperandKind.Label) },
            { LOAD_LIB, (OperandKind.Label, OperandKind.Label) },
            { ADD, (OperandKind.None, OperandKind.None) },
            { SUB, (OperandKind.None, OperandKind.None) },
            { MUL, (OperandKind.None, OperandKind.None) },
            { DIV, (OperandKind.None, OperandKind.None) },
            { MOD, (OperandKind.None, OperandKind.None) },
            { EQ, (OperandKind.None, OperandKind.None) },
            { NE, (OperandKind.None, OperandKind.None) },
            { LT, (OperandKind.None, OperandKind.None) },
            { GT, (OperandKind.None, OperandKind.None) },
            { LE, (OperandKind.None, OperandKind.None) },
            { GE, (OperandKind.None, OperandKind.None) },
        };

        public static Dictionary<string, (OperandKind, OperandKind)> InstructionSet { get => instructionSet; }

        public string Label { get; set; }
        public string Name { get; }
        public BaseValue Operand1 { get; }
        public BaseValue Operand2 { get; }

        private Instruction(string label, string name, BaseValue operand1, BaseValue operand2)
        {
            this.Label = label;
            this.Name = name;
            this.Operand1 = operand1;
            this.Operand2 = operand2;
        }

        public static Instruction Create(string label, string name, string operand1, string operand2)
        {
            if (IsValid(name, operand1, operand2, out BaseValue operand1Value, out BaseValue operand2Value))
            {
                return new Instruction(label, name, operand1Value, operand2Value);
            }
            else
            {
                return null;
            }
        }

        private static bool IsValid(string name, string operand1, string operand2, out BaseValue operand1Value, out BaseValue operand2Value)
        {
            operand1Value = null;
            operand2Value = null;
            if (name == null || !Instruction.InstructionSet.ContainsKey(name))
                return false;
            var (operand1Kind, operand2Kind) = InstructionSet[name];
            bool ret = IsValidOperandKind(operand1Kind, operand1, out operand1Value)
                && IsValidOperandKind(operand2Kind, operand2, out operand2Value);
            return ret;
        }

        private static bool IsValidOperandKind(OperandKind kind, string operand, out BaseValue operandValue)
        {
            operandValue = null;
            switch (kind)
            {
                case OperandKind.None:
                    if (!(operand is null))
                        return false;
                    break;
                case OperandKind.Identifier:
                case OperandKind.Label:
                    if (string.IsNullOrEmpty(operand))
                        return false;
                    operandValue = new StringValue(operand);
                    break;
                case OperandKind.Integer:
                    if (string.IsNullOrEmpty(operand))
                        return false;
                    var n = NumberValue.Parse(operand);
                    operandValue = new NumberValue(Math.Floor(n.Value));
                    break;
                case OperandKind.Number:
                    if (string.IsNullOrEmpty(operand))
                        return false;
                    operandValue = NumberValue.Parse(operand);
                    break;
                case OperandKind.String:
                    if (operand is null)
                        return false;
                    operandValue = new StringValue(operand);
                    break;
                case OperandKind.Boolean:
                    if (string.IsNullOrEmpty(operand))
                        return false;
                    operandValue = BooleanValue.ParseBooleanOperand(operand);
                    break;

            }
            return true;
        }

        public string ToDisplayString()
        {
            StringBuilder sb = new StringBuilder();
            if (this.Label != null)
            {
                sb.Append(this.Label);
                sb.Append(':');
                sb.Append('\n');
            }
            sb.Append(new String(' ', 4));
            sb.Append(this.Name);
            var (operand1Kind, operand2Kind) = InstructionSet[this.Name];

            if (operand1Kind != OperandKind.None)
            {
                sb.Append(' ');
                sb.Append(FormatOperand(operand1Kind, this.Operand1));
            }
            if (operand2Kind != OperandKind.None)
            {
                sb.Append(' ');
                sb.Append(FormatOperand(operand2Kind, this.Operand2));
            }
            return sb.ToString();
        }

        private static string FormatOperand(OperandKind kind, BaseValue operand)
        {
            switch (kind)
            {
                case OperandKind.Identifier:
                case OperandKind.Label:
                case OperandKind.Number:
                    return operand.ToDisplayString();
                case OperandKind.String:
                    return "\"" + StringValue.Escape(operand.ToString()) + "\"";
                case OperandKind.Boolean:
                    return operand.ToDisplayString();
                case OperandKind.Integer:
                    return ((int)operand.ToNumber()).ToString();
                default:
                    return null;
            }
        }

        public sealed override string ToString() => this.ToDisplayString();
    }
}
