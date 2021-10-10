// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ISB.Scanning;

namespace ISB.Runtime
{
    public sealed class Assembly : IEnumerable<Instruction>
    {
        public List<Instruction> Instructions { get; private set; }
        public List<TextRange> SourceMap { get; private set; }

        public Assembly()
        {
            this.Instructions = new List<Instruction>();
            this.SourceMap = new List<TextRange>();
        }

        public int Count => Instructions.Count;

        public Instruction this[int index] => this.Instructions[index];

        public TextRange LookupSourceMap(int IP)
        {
            if (IP >= 0 && IP < this.SourceMap.Count)
                return this.SourceMap[IP];
            else
                return TextRange.None;
        }

        // For incremental compilation - merge one assembly to another.
        // baseSourceLineNo is the start line no of the mapped source lines.
        public void Append(Assembly newAssembly, int baseSourceLineNo)
        {
            foreach (var i in newAssembly.Instructions)
            {
                this.Instructions.Add(i);
            }
            foreach (var i in newAssembly.SourceMap)
            {
                if (i != TextRange.None)
                    this.SourceMap.Add(
                        ((i.Start.Line + baseSourceLineNo, i.Start.Column),
                        (i.End.Line + baseSourceLineNo, i.End.Column)));
                else
                    this.SourceMap.Add(i);
            }
        }

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
            this.SourceMap.Clear();
        }

        public void Add(TextRange sourceRange, Instruction instruction)
        {
            if (instruction != null)
            {
                int IP = this.Instructions.Count;
                this.Instructions.Add(instruction);
                this.SourceMap.Add(sourceRange);
            }
        }

        public void Add(TextRange sourceRange, string label, string name, string operand1, string operand2)
        {
            this.Add(sourceRange, Instruction.Create(label, name, operand1, operand2));
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
            string[] lines = textFormatAssembly.Split(new string[] { "\r\n", "\r", "\n" },
                System.StringSplitOptions.None);
            string label = null;
            foreach (string line in lines)
            {
                string s = line.Trim();
                if (s.StartsWith(";"))
                {
                    // Ignores comments.
                }
                else if (s.EndsWith(":"))
                {
                    label = s.Substring(0, s.Length - 1);
                }
                else
                {
                    string[] tokens = TokenizeAsmLine(s);
                    string name = tokens.Length > 0 ? tokens[0] : null;
                    string operand1Token = tokens.Length > 1 ? tokens[1] : null;
                    string operand2Token = tokens.Length > 2 ? tokens[2] : null;
                    var instruction = Instruction.Create(label, name, operand1Token, operand2Token);
                    if (instruction != null)
                    {
                        assembly.Add(TextRange.None, instruction);
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
