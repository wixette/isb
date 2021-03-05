// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Utilities;

namespace ISB.Runtime
{
    public sealed class Engine
    {
        private Environment env;
        private string moduleName;
        private DiagnosticBag diagnostics;
        private Assembly assembly;

        public Engine(string moduleName)
        {
            this.env = new Environment();
            this.moduleName = moduleName;
            this.diagnostics = new DiagnosticBag();
            this.assembly = new Assembly();
            this.CodeLines = new List<string>();
        }

        public List<string> CodeLines { get; private set; }

        public bool HasError => this.diagnostics.Contents.Count > 0;

        public DiagnosticBag ErrorInfo => this.diagnostics;

        public string AssemblyInTextFormat => this.assembly.ToTextFormat();

        public int IP => this.env.IP;

        public int StackCount => this.env.RuntimeStack.Count;

        public BaseValue StackTop =>
            this.env.RuntimeStack.Count > 0 ? (BaseValue)this.env.RuntimeStack.Peek().Clone() : null;

        public bool Compile(string code, bool init)
        {
            if (!init)
            {
                this.env.Reset();
                this.assembly.Clear();
            }
            this.diagnostics.Reset();
            var tokens = Scanner.Scan(code, diagnostics);
            var tree = Parser.Parse(tokens, diagnostics);
            if (this.HasError)
            {
                return false;
            }
            Assembly newAssembly = Compiler.Compile(tree, this.env, this.moduleName, this.diagnostics,
                out HashSet<string> newCompileTimeLabels, out HashSet<string> newCompileTimeSubNames);
            if (this.HasError)
            {
                return false;
            }
            int baseLineNo = this.CodeLines.Count;
            MergeAssembly(this.assembly, newAssembly, baseLineNo);
            string[] newCodeLines = code.Split(new string[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);
            MergeCodeLines(this.CodeLines, newCodeLines);
            MergeSymbolSet(this.env.CompileTimeLabels, newCompileTimeLabels);
            MergeSymbolSet(this.env.CompileTimeSubNames, newCompileTimeSubNames);
            return true;
        }

        private static void MergeCodeLines(List<string> to, string[] from)
        {
            to.AddRange(from);
        }

        private static void MergeAssembly(Assembly to, Assembly from, int baseLineNo)
        {
            foreach (var i in from.Instructions)
            {
                to.Instructions.Add(i);
            }
            // The line numbers in the source map need to be updated
            foreach (var i in from.SourceMap)
            {
                if (i != TextRange.None)
                    to.SourceMap.Add(
                        ((i.Start.Line + baseLineNo, i.Start.Column),
                        (i.Start.Line + baseLineNo, i.Start.Column)));
                else
                    to.SourceMap.Add(i);
            }
        }

        private static void MergeSymbolSet(HashSet<string> to, HashSet<string> from)
        {
            foreach (var k in from)
                to.Add(k);
        }

        // Parsing assembly does not support incremental mode. Once a new assebmly is parsed, the existing instructions
        // and environment states are cleared.
        public void ParseAssembly(string assemblyCode)
        {
            this.env.Reset();
            this.diagnostics.Reset();
            this.assembly = Compiler.ParseAssembly(assemblyCode);
            // TODO: reports errors.
        }

        public bool Run(bool init)
        {
            if (init)
            {
                this.env.Reset();
            }
            this.diagnostics.Reset();
            this.ExecuteAssembly();
            return !this.HasError;
        }

        private void ExecuteAssembly()
        {
            // Scans for new labels and updates the label dictionary.
            for (int i = this.env.IP; i < this.assembly.Instructions.Count; i++)
            {
                var instruction = this.assembly.Instructions[i];
                if (instruction.Label != null)
                {
                    this.env.RuntimeLabels.Add(instruction.Label, i);
                }
            }

            // Executes instructions one by one.
            while (this.env.IP < this.assembly.Instructions.Count && !this.HasError)
            {
                Instruction instruction = this.assembly.Instructions[this.env.IP];
                ExecuteInstruction(instruction);
            }
        }

        private void ReportRuntimeError(string description)
        {
            this.diagnostics.Add(Diagnostic.ReportRuntimeError(
                this.assembly.LookupSourceMap(this.IP),
                $"{description} ({this.IP}: {this.assembly.Instructions[this.env.IP].ToDisplayString()})"));
        }

        private void ReportUndefinedAssemblyLabel(string label)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Undefined assembly label, {label}");
        }

        private void ReportEmptyStack()
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Unexpected empty stack.");
        }

        private void ReportInvalidValue(string value)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Invalid Value {value}.");
        }

        private void ReportDivisionByZero()
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Division by zero.");
        }

        private void ExecuteInstruction(Instruction instruction)
        {
            switch (instruction.Name)
            {
                case Instruction.NOP:
                {
                    // Does nothing.
                    this.env.IP++;
                    break;
                }

                case Instruction.PAUSE:
                {
                    // TODO
                    break;
                }

                case Instruction.BR:
                {
                    string label = instruction.Oprand1.ToString();
                    int target = this.env.RuntimeLabelToIP(label);
                    if (target >= 0)
                    {
                        this.env.IP = target;
                    }
                    else
                    {
                        this.ReportUndefinedAssemblyLabel(label);
                    }
                    break;
                }

                case Instruction.BR_IF:
                {
                    string label1 = instruction.Oprand1.ToString();
                    int target1 = this.env.RuntimeLabelToIP(label1);
                    string label2 = instruction.Oprand2.ToString();
                    int target2 = this.env.RuntimeLabelToIP(label2);
                    if (target1 >= 0 && target2 >= 0)
                    {
                        var value = this.env.RuntimeStack.Pop();
                        this.env.IP = value.ToBoolean() ? target1 : target2;
                    }
                    else
                    {
                        if (target1 < 0)
                            this.ReportUndefinedAssemblyLabel(label1);
                        if (target2 < 0)
                            this.ReportUndefinedAssemblyLabel(label2);
                    }
                    break;
                }

                case Instruction.SET:
                {
                    if (this.env.RuntimeStack.Count > 0)
                    {
                        var value = this.env.RuntimeStack.Pop();
                        this.env.SetRegister(instruction.Oprand1, value);
                        this.env.IP++;
                    }
                    else
                    {
                        this.ReportEmptyStack();
                    }
                    break;
                }

                case Instruction.GET:
                {
                    var value = this.env.GetRegister(instruction.Oprand1);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.STORE:
                {
                    if (this.env.RuntimeStack.Count > 0)
                    {
                        var value = this.env.RuntimeStack.Pop();
                        this.env.SetMemory(instruction.Oprand1, value);
                        this.env.IP++;
                    }
                    else
                    {
                        this.ReportEmptyStack();
                    }
                    break;
                }

                case Instruction.LOAD:
                {
                    var value = this.env.GetMemory(instruction.Oprand1);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.STORE_ARR:
                {
                    BaseValue[] arrayNameAndIndices;
                    if (this.PrepareArrayNameAndIndices(instruction, out arrayNameAndIndices))
                    {
                        if (this.env.RuntimeStack.Count > 0)
                        {
                            var value = this.env.RuntimeStack.Pop();
                            this.env.SetArrayItem(arrayNameAndIndices, value);
                            this.env.IP++;
                        }
                        else
                        {
                            this.ReportEmptyStack();
                        }
                    }
                    break;
                }

                case Instruction.LOAD_ARR:
                {
                    BaseValue[] arrayNameAndIndices;
                    if (this.PrepareArrayNameAndIndices(instruction, out arrayNameAndIndices))
                    {
                        var value = this.env.GetArrayItem(arrayNameAndIndices);
                        this.env.RuntimeStack.Push(value);
                        this.env.IP++;
                    }
                    break;
                }

                case Instruction.PUSH:
                {
                    var value = instruction.Oprand1;
                    Debug.Assert(value is NumberValue);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.PUSHS:
                {
                    var value = instruction.Oprand1;
                    Debug.Assert(value is StringValue);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.CALL:
                {
                    // TODO
                    break;
                }

                case Instruction.RET:
                {
                    // TODO
                    break;
                }

                case Instruction.CALL_LIB:
                {
                    // TODO
                    break;
                }

                case Instruction.STORE_LIB:
                {
                    // TODO
                    break;
                }

                case Instruction.LOAD_LIB:
                {
                    // TODO
                    break;
                }

                case Instruction.ADD:
                {
                    if (this.BinaryOperation((op1, op2) => op1 + op2, false))
                        this.env.IP++;
                    break;
                }

                case Instruction.SUB:
                {
                    if (this.BinaryOperation((op1, op2) => op1 - op2, false))
                        this.env.IP++;
                    break;
                }

                case Instruction.MUL:
                {
                    if (this.BinaryOperation((op1, op2) => op1 * op2, false))
                        this.env.IP++;
                    break;
                }

                case Instruction.DIV:
                {
                    if (this.BinaryOperation((op1, op2) => op1 / op2, true))
                        this.env.IP++;
                    break;
                }

                case Instruction.MOD:
                {
                    if (this.BinaryOperation((op1, op2) => op1 % op2, true))
                        this.env.IP++;
                    break;
                }

                case Instruction.EQ:
                {
                    if (this.BinaryLogicalOperation((op1, op2) => op1 == op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.NE:
                {
                    if (this.BinaryLogicalOperation((op1, op2) => op1 != op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.LT:
                {
                    if (this.BinaryLogicalOperation((op1, op2) => op1 < op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.GT:
                {
                    if (this.BinaryLogicalOperation((op1, op2) => op1 > op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.LE:
                {
                    if (this.BinaryLogicalOperation((op1, op2) => op1 <= op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.GE:
                {
                    if (this.BinaryLogicalOperation((op1, op2) => op1 >= op2))
                        this.env.IP++;
                    break;
                }
            }
        }

        private bool PrepareArrayNameAndIndices(Instruction instruction, out BaseValue[] arrayNameAndIndices)
        {
            int dimension = (int)Math.Floor(instruction.Oprand2.ToNumber());
            if (dimension <= 0)
            {
                this.ReportInvalidValue(dimension.ToString());
                arrayNameAndIndices = null;
                return false;
            }
            arrayNameAndIndices = new BaseValue[dimension + 1];
            arrayNameAndIndices[0] = instruction.Oprand1;
            for (int i = 1; i < dimension + 1; i++)
            {
                if (this.env.RuntimeStack.Count <= 0)
                {
                    this.ReportEmptyStack();
                    return false;
                }
                arrayNameAndIndices[i] = this.env.RuntimeStack.Pop();
            }
            return true;
        }

        private bool BinaryOperation(Func<decimal, decimal, decimal> operation, bool checkDivisionByZero)
        {
            if (this.env.RuntimeStack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            decimal op2 = this.env.RuntimeStack.Pop().ToNumber();
            decimal op1 = this.env.RuntimeStack.Pop().ToNumber();
            if (checkDivisionByZero && op2 == 0)
            {
                this.ReportDivisionByZero();
                return false;
            }
            decimal result = operation(op1, op2);
            this.env.RuntimeStack.Push(new NumberValue(result));
            return true;
        }

        private bool BinaryLogicalOperation(Func<decimal, decimal, bool> operation)
        {
            if (this.env.RuntimeStack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            decimal op2 = this.env.RuntimeStack.Pop().ToNumber();
            decimal op1 = this.env.RuntimeStack.Pop().ToNumber();
            bool result = operation(op1, op2);
            this.env.RuntimeStack.Push(new BooleanValue(result));
            return true;
        }
    }
}