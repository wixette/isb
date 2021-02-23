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
            Value
        }

        private static readonly Dictionary<string, (OprandKind, OprandKind)> instructionSet =
            new Dictionary<string, (OprandKind, OprandKind)>()
        {
            { "nop", (OprandKind.None, OprandKind.None) },
            { "pause", (OprandKind.None, OprandKind.None) },
            { "br", (OprandKind.Label, OprandKind.None) },
            { "br_if", (OprandKind.Label, OprandKind.Label) },
            { "store", (OprandKind.Identifier, OprandKind.None) },
            { "load", (OprandKind.Identifier, OprandKind.None) },
            { "store_arr", (OprandKind.Identifier, OprandKind.Integer) },
            { "load_arr", (OprandKind.Identifier, OprandKind.Integer) },
            { "push", (OprandKind.Value, OprandKind.None) },
            { "call", (OprandKind.Label, OprandKind.None) },
            { "ret", (OprandKind.Label, OprandKind.None) },
            { "call_lib", (OprandKind.Label, OprandKind.Label) },
            { "store_lib", (OprandKind.Label, OprandKind.Label) },
            { "load_lib", (OprandKind.Label, OprandKind.Label) },
            { "add", (OprandKind.None, OprandKind.None) },
            { "sub", (OprandKind.None, OprandKind.None) },
            { "mul", (OprandKind.None, OprandKind.None) },
            { "div", (OprandKind.None, OprandKind.None) },
            { "eq", (OprandKind.None, OprandKind.None) },
            { "ne", (OprandKind.None, OprandKind.None) },
            { "lt", (OprandKind.None, OprandKind.None) },
            { "gt", (OprandKind.None, OprandKind.None) },
            { "le", (OprandKind.None, OprandKind.None) },
            { "ge", (OprandKind.None, OprandKind.None) },
        };

        public static Dictionary<string, (OprandKind, OprandKind)> InstructionSet { get => instructionSet; }

        public string Label { get; }
        public string Name { get; }
        public BaseValue Oprand1 { get; }
        public BaseValue Oprand2 { get; }

        public Instruction(string label, string name, BaseValue oprand1, BaseValue oprand2)
        {
            Debug.Assert(Instruction.IsValid(name, oprand1, oprand2));
            this.Label = label;
            this.Name = name;
            this.Oprand1 = oprand1;
            this.Oprand2 = oprand2;
        }

        public static bool IsValid(string name, BaseValue oprand1, BaseValue oprand2)
        {
            if (name == null || !Instruction.InstructionSet.ContainsKey(name))
                return false;
            var (oprand1Kind, oprand2Kind) = InstructionSet[name];
            bool ret = IsValidOprandKind(oprand1Kind, oprand1) && IsValidOprandKind(oprand2Kind, oprand2);
            return ret;
        }

        private static bool IsValidOprandKind(OprandKind kind, BaseValue oprand)
        {
            switch (kind)
            {
                case OprandKind.None:
                    if (oprand != null)
                        return false;
                    break;
                case OprandKind.Identifier:
                case OprandKind.Label:
                    if (oprand == null || !(oprand is StringValue))
                        return false;
                    break;
                case OprandKind.Integer:
                    if (oprand == null || !(oprand is NumberValue))
                        return false;
                    break;
                case OprandKind.Value:
                    if (oprand == null)
                        return false;
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
                sb.Append(',');
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
                case OprandKind.Value:
                    return oprand.ToDisplayString();
                case OprandKind.Integer:
                    return ((int)oprand.ToNumber()).ToString();
                default:
                    return null;
            }
        }

        public sealed override string ToString() => this.ToDisplayString();
    }
}
