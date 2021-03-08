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
        public enum OprandKind
        {
            None = 0,
            Label,
            Identifier,
            Integer,
            Number,
            String
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

        // Adds two oprands.
        //   add
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 + value2
        //   (4) Stack.Push(result)
        public const string ADD = "add";

        // Substracts two oprands.
        //   sub
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 - value2
        //   (4) Stack.Push(result)
        public const string SUB = "sub";

        // Times two oprands.
        //   mul
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 * value2
        //   (4) Stack.Push(result)
        public const string MUL = "mul";

        // Divides an oprand by another.
        //   div
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 / value2
        //   (4) Stack.Push(result)
        public const string DIV = "div";

        // Modulo. Divides an oprand by another and returns the reminder.
        //   mod
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 % value2
        //   (4) Stack.Push(result)
        public const string MOD = "mod";

        // Determinates if two values are equal.
        //   eq
        //
        //   (1) value2 := Stack.Pop()
        //   (2) value1 := Stack.Pop()
        //   (3) result = value1 == value2
        //   (4) Stack.Push(result)
        public const string EQ = "eq";

        // Determinates if two values are not equal.
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
        public const string TrueLiteral = "1";
        public const string FalseLiteral = "0";

        private static readonly Dictionary<string, (OprandKind, OprandKind)> instructionSet =
            new Dictionary<string, (OprandKind, OprandKind)>()
        {
            { NOP, (OprandKind.None, OprandKind.None) },
            { BR, (OprandKind.Label, OprandKind.None) },
            { BR_IF, (OprandKind.Label, OprandKind.Label) },
            { SET, (OprandKind.Integer, OprandKind.None) },
            { GET, (OprandKind.Integer, OprandKind.None) },
            { STORE, (OprandKind.Identifier, OprandKind.None) },
            { LOAD, (OprandKind.Identifier, OprandKind.None) },
            { STORE_ARR, (OprandKind.Identifier, OprandKind.Integer) },
            { LOAD_ARR, (OprandKind.Identifier, OprandKind.Integer) },
            { PUSH, (OprandKind.Number, OprandKind.None) },
            { PUSHS, (OprandKind.String, OprandKind.None) },
            { CALL, (OprandKind.Label, OprandKind.None) },
            { RET, (OprandKind.Integer, OprandKind.None) },
            { CALL_LIB, (OprandKind.Label, OprandKind.Label) },
            { STORE_LIB, (OprandKind.Label, OprandKind.Label) },
            { LOAD_LIB, (OprandKind.Label, OprandKind.Label) },
            { ADD, (OprandKind.None, OprandKind.None) },
            { SUB, (OprandKind.None, OprandKind.None) },
            { MUL, (OprandKind.None, OprandKind.None) },
            { DIV, (OprandKind.None, OprandKind.None) },
            { MOD, (OprandKind.None, OprandKind.None) },
            { EQ, (OprandKind.None, OprandKind.None) },
            { NE, (OprandKind.None, OprandKind.None) },
            { LT, (OprandKind.None, OprandKind.None) },
            { GT, (OprandKind.None, OprandKind.None) },
            { LE, (OprandKind.None, OprandKind.None) },
            { GE, (OprandKind.None, OprandKind.None) },
        };

        public static Dictionary<string, (OprandKind, OprandKind)> InstructionSet { get => instructionSet; }

        public string Label { get; }
        public string Name { get; }
        public BaseValue Oprand1 { get; }
        public BaseValue Oprand2 { get; }

        private Instruction(string label, string name, BaseValue oprand1, BaseValue oprand2)
        {
            this.Label = label;
            this.Name = name;
            this.Oprand1 = oprand1;
            this.Oprand2 = oprand2;
        }

        public static Instruction Create(string label, string name, string oprand1, string oprand2)
        {
            if (IsValid(name, oprand1, oprand2, out BaseValue oprand1Value, out BaseValue oprand2Value))
            {
                return new Instruction(label, name, oprand1Value, oprand2Value);
            }
            else
            {
                return null;
            }
        }

        private static bool IsValid(string name, string oprand1, string oprand2, out BaseValue oprand1Value, out BaseValue oprand2Value)
        {
            oprand1Value = null;
            oprand2Value = null;
            if (name == null || !Instruction.InstructionSet.ContainsKey(name))
                return false;
            var (oprand1Kind, oprand2Kind) = InstructionSet[name];
            bool ret = IsValidOprandKind(oprand1Kind, oprand1, out oprand1Value)
                && IsValidOprandKind(oprand2Kind, oprand2, out oprand2Value);
            return ret;
        }

        private static bool IsValidOprandKind(OprandKind kind, string oprand, out BaseValue oprandValue)
        {
            oprandValue = null;
            switch (kind)
            {
                case OprandKind.None:
                    if (oprand != null)
                        return false;
                    break;
                case OprandKind.Identifier:
                case OprandKind.Label:
                    if (oprand == null || oprand.Length <= 0)
                        return false;
                    oprandValue = new StringValue(oprand);
                    break;
                case OprandKind.Integer:
                    if (oprand == null || oprand.Length <= 0)
                        return false;
                    var n = NumberValue.Parse(oprand);
                    oprandValue = new NumberValue(Math.Floor(n.Value));
                    break;
                case OprandKind.Number:
                    if (oprand == null || oprand.Length <= 0)
                        return false;
                    oprandValue = NumberValue.Parse(oprand);
                    break;
                case OprandKind.String:
                    if (oprand == null || oprand.Length <= 0)
                        return false;
                    oprandValue = new StringValue(oprand);
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
            var (oprand1Kind, oprand2Kind) = InstructionSet[this.Name];

            if (oprand1Kind != OprandKind.None)
            {
                sb.Append(' ');
                sb.Append(FormatOprand(oprand1Kind, this.Oprand1));
            }
            if (oprand2Kind != OprandKind.None)
            {
                sb.Append(' ');
                sb.Append(FormatOprand(oprand2Kind, this.Oprand2));
            }
            return sb.ToString();
        }

        private static string FormatOprand(OprandKind kind, BaseValue oprand)
        {
            switch (kind)
            {
                case OprandKind.Identifier:
                case OprandKind.Label:
                case OprandKind.Number:
                    return oprand.ToDisplayString();
                case OprandKind.String:
                    return "\"" + StringValue.Escape(oprand.ToString()) + "\"";
                case OprandKind.Integer:
                    return ((int)oprand.ToNumber()).ToString();
                default:
                    return null;
            }
        }

        public sealed override string ToString() => this.ToDisplayString();
    }
}
