// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Diagnostics;
using ISB.Scanning;
using ISB.Parsing;
using ISB.Utilities;

namespace ISB.Runtime
{
    public sealed class Engine
    {
        private Environment env;
        private DiagnosticBag diagnostics;
        private Compiler compiler;

        public Engine()
        {
            this.env = new Environment();
            this.diagnostics = new DiagnosticBag();
            this.compiler = new Compiler(this.env, "Program", this.diagnostics);
        }

        public bool HasError => this.diagnostics.Contents.Count > 0;

        public DiagnosticBag ErrorInfo => this.diagnostics;

        public string AssemblyInTextFormat => compiler.Instructions.ToTextFormat();

        public int IP => this.env.IP;

        public int StackCount => this.env.Stack.Count;

        public BaseValue StackTop => (BaseValue)this.env.Stack.Peek().Clone();

        public bool Compile(string code, bool init)
        {
            if (!init)
            {
                this.env.Reset();
                this.compiler.Reset();
            }
            this.diagnostics.Reset();
            var tokens = Scanner.Scan(code, diagnostics);
            var tree = Parser.Parse(tokens, diagnostics);
            if (this.HasError)
            {
                // So far we don't mix the stage of scanning/parsing and the stage of IR generating.
                // TODO: reports errors.
                return false;
            }
            this.compiler.Compile(tree);
            return !this.HasError;
        }

        // Parsing assembly does not support incremental mode. Once a new assebmly is parsed, the existing instructions
        // and environment states are cleared.
        public void ParseAssembly(string assemblyCode)
        {
            this.env.Reset();
            this.compiler.Reset();
            this.diagnostics.Reset();
            this.compiler.ParseAssembly(assemblyCode);
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
            for (int i = this.env.IP; i < this.compiler.Instructions.Count; i++)
            {
                var instruction = this.compiler.Instructions[i];
                if (instruction.Label != null)
                {
                    this.env.Labels.Add(instruction.Label, i);
                }
            }

            // Executes instructions one by one.
            while (this.env.IP < this.compiler.Instructions.Count && !this.HasError)
            {
                Instruction instruction = this.compiler.Instructions[this.env.IP];
                ExecuteInstruction(instruction);
            }
        }

        private void ReportRuntimeError(string description)
        {
            this.diagnostics.Add(Diagnostic.ReportRuntimeError(
                this.compiler.LookupSourceMap(this.IP),
                $"{description} ({this.IP}: {this.compiler.Instructions[this.env.IP].ToDisplayString()})"));
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
                    int target = this.env.LookupLabel(label);
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
                    int target1 = this.env.LookupLabel(label1);
                    string label2 = instruction.Oprand2.ToString();
                    int target2 = this.env.LookupLabel(label2);
                    if (target1 >= 0 && target2 >= 0)
                    {
                        var value = this.env.Stack.Pop();
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
                    if (this.env.Stack.Count > 0)
                    {
                        var value = this.env.Stack.Pop();
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
                    this.env.Stack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.STORE:
                {
                    if (this.env.Stack.Count > 0)
                    {
                        var value = this.env.Stack.Pop();
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
                    this.env.Stack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.STORE_ARR:
                {
                    BaseValue[] arrayNameAndIndices;
                    if (this.PrepareArrayNameAndIndices(instruction, out arrayNameAndIndices))
                    {
                        if (this.env.Stack.Count > 0)
                        {
                            var value = this.env.Stack.Pop();
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
                        this.env.Stack.Push(value);
                        this.env.IP++;
                    }
                    break;
                }

                case Instruction.PUSH:
                {
                    var value = instruction.Oprand1;
                    Debug.Assert(value is NumberValue);
                    this.env.Stack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.PUSHS:
                {
                    var value = instruction.Oprand1;
                    Debug.Assert(value is StringValue);
                    this.env.Stack.Push(value);
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
                    if (this.BinaryOperation((op1, op2) => op1 + op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.SUB:
                {
                    if (this.BinaryOperation((op1, op2) => op1 - op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.MUL:
                {
                    if (this.BinaryOperation((op1, op2) => op1 * op2))
                        this.env.IP++;
                    break;
                }

                case Instruction.DIV:
                {
                    if (this.BinaryOperation((op1, op2) => op1 / op2))
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
                if (this.env.Stack.Count <= 0)
                {
                    this.ReportEmptyStack();
                    return false;
                }
                arrayNameAndIndices[i] = this.env.Stack.Pop();
            }
            return true;
        }

        private bool BinaryOperation(Func<decimal, decimal, decimal> operation)
        {
            if (this.env.Stack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            decimal op2 = this.env.Stack.Pop().ToNumber();
            decimal op1 = this.env.Stack.Pop().ToNumber();
            decimal result = operation(op1, op2);
            this.env.Stack.Push(new NumberValue(result));
            return true;
        }

        private bool BinaryLogicalOperation(Func<decimal, decimal, bool> operation)
        {
            if (this.env.Stack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            decimal op2 = this.env.Stack.Pop().ToNumber();
            decimal op1 = this.env.Stack.Pop().ToNumber();
            bool result = operation(op1, op2);
            this.env.Stack.Push(new BooleanValue(result));
            return true;
        }
    }
}