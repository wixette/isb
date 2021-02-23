// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

namespace ISB.Runtime
{
    public sealed class Assembly
    {
        public List<Instruction> InstructionSequence { get; }

        public Assembly()
        {
            this.InstructionSequence = new List<Instruction>();
        }

        public void AddInstruction(Instruction instruction)
        {
            this.InstructionSequence.Add(instruction);
        }

        public string ToDisplayString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var instruction in this.InstructionSequence)
            {
                sb.AppendLine(instruction.ToDisplayString());
            }
            return sb.ToString();
        }

        public sealed override string ToString() => this.ToDisplayString();

        public string ToTextFormat() => this.ToDisplayString();

        public static Assembly Parse(string textFormatAssembly)
        {
            Assembly assembly = new Assembly();
            string[] lines = textFormatAssembly.Split('\n');
            string label = null;
            foreach (string line in lines)
            {
                string s = line.Trim(new char[] { ' ', '\t', '\r', '\n' });
                if (s.StartsWith('#'))
                {
                    // Ignores comments.
                }
                else if (s.EndsWith(':'))
                {
                    label = s.Substring(0, s.Length - 1);
                }
                else
                {
                    string[] tokens = s.Split(new char[] { ' ', '\t' });
                    string name = tokens.Length > 0 ? tokens[0] : null;
                    string oprand1Token = tokens.Length > 1 ? tokens[1] : null;
                    string oprand2Token = tokens.Length > 2 ? tokens[2] : null;
                    BaseValue oprand1 = oprand1Token != null ? StringValue.Parse(oprand1Token) : null;
                    BaseValue oprand2 = oprand2Token != null ? StringValue.Parse(oprand2Token) : null;
                    if (Instruction.IsValid(name, oprand1, oprand2))
                    {
                        assembly.AddInstruction(new Instruction(label, name, oprand1, oprand2));
                        label = null;
                    }
                }
            }
            return assembly;
        }
    }
}
