// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Text;

namespace ISB.Runtime
{
    public class Assembly
    {
        public List<Instruction> InstructionSequence { get; }

        public Assembly()
        {
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
    }
}
