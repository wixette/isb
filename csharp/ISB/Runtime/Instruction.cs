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

        public static readonly string NOP = "nop";
        public static readonly string PAUSE = "pause";
        public static readonly string BR = "br";
        public static readonly string BR_IF = "br_if";
        public static readonly string STORE = "store";
        public static readonly string LOAD = "load";
        public static readonly string STORE_ARR = "store_arr";
        public static readonly string LOAD_ARR = "load_arr";
        public static readonly string PUSH = "push";
        public static readonly string PUSHS = "pushs";
        public static readonly string CALL = "call";
        public static readonly string RET = "ret";
        public static readonly string CALL_LIB = "call_lib";
        public static readonly string STORE_LIB = "store_lib";
        public static readonly string LOAD_LIB = "load_lib";
        public static readonly string ADD = "add";
        public static readonly string SUB = "sub";
        public static readonly string MUL = "mul";
        public static readonly string DIV = "div";
        public static readonly string EQ = "eq";
        public static readonly string NE = "ne";
        public static readonly string LT = "lt";
        public static readonly string GT = "gt";
        public static readonly string LE = "le";
        public static readonly string GE = "ge";

        public static readonly string ZeroLiteral = "0";
        public static readonly string TrueLiteral = "1";
        public static readonly string FalseLiteral = "0";

        private static readonly Dictionary<string, (OprandKind, OprandKind)> instructionSet =
            new Dictionary<string, (OprandKind, OprandKind)>()
        {
            { NOP, (OprandKind.None, OprandKind.None) },
            { PAUSE, (OprandKind.None, OprandKind.None) },
            { BR, (OprandKind.Label, OprandKind.None) },
            { BR_IF, (OprandKind.Label, OprandKind.Label) },
            { STORE, (OprandKind.Identifier, OprandKind.None) },
            { LOAD, (OprandKind.Identifier, OprandKind.None) },
            { STORE_ARR, (OprandKind.Identifier, OprandKind.Integer) },
            { LOAD_ARR, (OprandKind.Identifier, OprandKind.Integer) },
            { PUSH, (OprandKind.Number, OprandKind.None) },
            { PUSHS, (OprandKind.String, OprandKind.None) },
            { CALL, (OprandKind.Label, OprandKind.None) },
            { RET, (OprandKind.None, OprandKind.None) },
            { CALL_LIB, (OprandKind.Label, OprandKind.Label) },
            { STORE_LIB, (OprandKind.Label, OprandKind.Label) },
            { LOAD_LIB, (OprandKind.Label, OprandKind.Label) },
            { ADD, (OprandKind.None, OprandKind.None) },
            { SUB, (OprandKind.None, OprandKind.None) },
            { MUL, (OprandKind.None, OprandKind.None) },
            { DIV, (OprandKind.None, OprandKind.None) },
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
