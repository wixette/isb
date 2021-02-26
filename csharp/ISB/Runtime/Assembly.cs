// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ISB.Runtime
{
    public sealed class Assembly : IEnumerable<Instruction>
    {
        public List<Instruction> Instructions { get; private set; }

        public Assembly()
        {
            this.Instructions = new List<Instruction>();
        }

        public int Count { get=> Instructions.Count; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<Instruction> GetEnumerator()
        {
            return this.Instructions.GetEnumerator();
        }

        public void Clear()
        {
            this.Instructions.Clear();
        }

        public void Add(Instruction instruction)
        {
            this.Instructions.Add(instruction);
        }

        public void Add(string label, string name, BaseValue oprand1, BaseValue oprand2)
        {
            this.Instructions.Add(new Instruction(label, name, oprand1, oprand2));
        }

        public string ToDisplayString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var instruction in this.Instructions)
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
                    // TODO: Support quoted string literials like "Hello, World!" and escape strings.
                    string[] tokens = s.Split(new char[] { ' ', '\t' });
                    string name = tokens.Length > 0 ? tokens[0] : null;
                    string oprand1Token = tokens.Length > 1 ? tokens[1] : null;
                    string oprand2Token = tokens.Length > 2 ? tokens[2] : null;
                    BaseValue oprand1 = oprand1Token != null ? StringValue.Parse(oprand1Token) : null;
                    BaseValue oprand2 = oprand2Token != null ? StringValue.Parse(oprand2Token) : null;
                    if (Instruction.IsValid(name, oprand1, oprand2))
                    {
                        assembly.Add(label, name, oprand1, oprand2);
                        label = null;
                    }
                }
            }
            return assembly;
        }
    }
}
