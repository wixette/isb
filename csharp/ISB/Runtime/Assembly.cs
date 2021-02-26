// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
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

        public void Add(string label, string name, string oprand1, string oprand2)
        {
            var instruction = Instruction.Create(label, name, oprand1, oprand2);
            if (instruction != null)
                this.Instructions.Add(instruction);
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
            // This parser is only for debugging. It is not a fully-functional scanner/parser. It simply drops all
            // invalid input parts and try to produce as many out instructions as possible.
            Assembly assembly = new Assembly();
            string[] lines = textFormatAssembly.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.None);
            string label = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.StartsWith(';'))
                {
                    // Ignores comments.
                }
                else if (s.EndsWith(':'))
                {
                    label = s.Substring(0, s.Length - 1);
                }
                else
                {
                    string[] tokens = TokenizeAsmLine(s);
                    string name = tokens.Length > 0 ? tokens[0] : null;
                    string oprand1Token = tokens.Length > 1 ? tokens[1] : null;
                    string oprand2Token = tokens.Length > 2 ? tokens[2] : null;
                    var instruction = Instruction.Create(label, name, oprand1Token, oprand2Token);
                    if (instruction != null)
                    {
                        assembly.Add(instruction);
                        label = null;
                    }
                }
            }
            return assembly;
        }

        private static string[] TokenizeAsmLine(string line)
        {
            List<string> result = new List<string>();
            int i = 0;
            bool inWhiteSpaces = false;
            bool inQuote = false;
            StringBuilder buffer = new StringBuilder();
            while (i < line.Length)
            {
                char c = line[i];

                if (Char.IsWhiteSpace(c))
                {
                    if (inQuote)
                    {
                        buffer.Append(c);
                    }
                    else if (!inWhiteSpaces)
                    {
                        if (buffer.Length > 0) result.Add(buffer.ToString());
                        buffer.Clear();
                        inWhiteSpaces = true;
                    }
                }
                else // Non-whitespace
                {
                    if (c == '"')
                    {
                        inQuote = !inQuote;
                        if (buffer.Length > 0) result.Add(buffer.ToString());
                        buffer.Clear();
                    }
                    else if (c == '\\')
                    {
                        if (inQuote && i < line.Length - 1)
                        {
                            i++;
                            c = line[i];
                            if (c == '\\' || c == '"')
                            {
                                buffer.Append(c);
                            }
                        }
                    }
                    else
                    {
                        buffer.Append(c);
                    }
                    inWhiteSpaces = false;
                }

                i++;
            }
            if (buffer.Length > 0) result.Add(buffer.ToString());
            return result.ToArray();
        }
    }
}
