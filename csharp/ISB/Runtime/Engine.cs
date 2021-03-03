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

        private void ResetForCompiling(bool isIncremental)
        {
            this.diagnostics.Reset();
            if (!isIncremental)
            {
                this.env.Reset();
                this.compiler.Reset();
            }
        }

        private void ResetForRunning(bool isIncremental)
        {
            this.diagnostics.Reset();
            if (!isIncremental)
            {
                this.env.Reset();
            }
        }

        public bool HasError => this.diagnostics.Contents.Count > 0;

        public DiagnosticBag ErrorInfo => this.diagnostics;

        public string AssemblyInTextFormat => compiler.Instructions.ToTextFormat();

        public int IP => this.env.IP;

        public int StackCount => this.env.Stack.Count;

        public BaseValue StackTop => (BaseValue)this.env.Stack.Peek().Clone();

        public bool Compile(string code, bool isIncremental)
        {
            this.ResetForCompiling(isIncremental);
            var tokens = Scanner.Scan(code, diagnostics);
            var tree = Parser.Parse(tokens, diagnostics);
            if (this.HasError)
            {
                // So far we don't mix the stage of scanning/parsing and the stage of IR generating.
                return false;
            }
            this.compiler.Compile(tree);
            return !this.HasError;
        }

        // Parsing assembly does not support incremental mode. Once a new assebmly is parsed, the existing instructions
        // and environment states are cleared.
        public void ParseAssembly(string assemblyCode)
        {
            this.ResetForCompiling(true);
            this.compiler.ParseAssembly(assemblyCode);
        }

        public bool Run(bool isIncremental)
        {
            this.ResetForRunning(isIncremental);
            this.ExecuteAssembly();
            return !this.HasError;
        }

        private void ExecuteAssembly()
        {
            while (this.env.IP < this.compiler.Instructions.Count)
            {
                Instruction instruction = this.compiler.Instructions[this.env.IP];
                ExecuteInstruction(instruction);
            }
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
                    int target = this.env.LookupLabel(instruction.Oprand1.ToString());
                    Debug.Assert(target >= 0);
                    this.env.IP = target;
                    break;
                }

                case Instruction.BR_IF:
                {
                    int target1 = this.env.LookupLabel(instruction.Oprand1.ToString());
                    int target2 = this.env.LookupLabel(instruction.Oprand2.ToString());
                    Debug.Assert(target1 >= 0 && target2 >= 0);
                    var value = this.env.Stack.Pop();
                    this.env.IP = value.ToBoolean() ? target1 : target2;
                    break;
                }

                case Instruction.SET:
                {
                    var value = this.env.Stack.Pop();
                    this.env.SetRegister(instruction.Oprand1, value);
                    this.env.IP++;
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
                    var value = this.env.Stack.Pop();
                    this.env.SetMemory(instruction.Oprand1, value);
                    this.env.IP++;
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
                    BaseValue[] arrayNameAndIndices = this.PrepareArrayNameAndIndices(instruction);
                    var value = this.env.Stack.Pop();
                    this.env.SetArrayItem(arrayNameAndIndices, value);
                    this.env.IP++;
                    break;
                }

                case Instruction.LOAD_ARR:
                {
                    BaseValue[] arrayNameAndIndices = this.PrepareArrayNameAndIndices(instruction);
                    var value = this.env.GetArrayItem(arrayNameAndIndices);
                    this.env.Stack.Push(value);
                    this.env.IP++;
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
                    this.BinaryOperation((op1, op2) => op1 + op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.SUB:
                {
                    this.BinaryOperation((op1, op2) => op1 - op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.MUL:
                {
                    this.BinaryOperation((op1, op2) => op1 * op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.DIV:
                {
                    this.BinaryOperation((op1, op2) => op1 / op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.EQ:
                {
                    this.BinaryLogicalOperation((op1, op2) => op1 == op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.NE:
                {
                    this.BinaryLogicalOperation((op1, op2) => op1 != op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.LT:
                {
                    this.BinaryLogicalOperation((op1, op2) => op1 < op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.GT:
                {
                    this.BinaryLogicalOperation((op1, op2) => op1 > op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.LE:
                {
                    this.BinaryLogicalOperation((op1, op2) => op1 <= op2);
                    this.env.IP++;
                    break;
                }

                case Instruction.GE:
                {
                    this.BinaryLogicalOperation((op1, op2) => op1 >= op2);
                    this.env.IP++;
                    break;
                }
            }
        }

        private BaseValue[] PrepareArrayNameAndIndices(Instruction instruction)
        {
            int dimension = (int)Math.Floor(instruction.Oprand2.ToNumber());
            Debug.Assert(dimension > 0);
            BaseValue[] arrayNameAndIndices = new BaseValue[dimension + 1];
            arrayNameAndIndices[0] = instruction.Oprand1;
            for (int i = 1; i < dimension + 1; i++)
            {
                arrayNameAndIndices[i] = this.env.Stack.Pop();
            }
            return arrayNameAndIndices;
        }

        private void BinaryOperation(Func<decimal, decimal, decimal> operation)
        {
            decimal op2 = this.env.Stack.Pop().ToNumber();
            decimal op1 = this.env.Stack.Pop().ToNumber();
            decimal result = operation(op1, op2);
            this.env.Stack.Push(new NumberValue(result));
        }

        private void BinaryLogicalOperation(Func<decimal, decimal, bool> operation)
        {
            decimal op2 = this.env.Stack.Pop().ToNumber();
            decimal op1 = this.env.Stack.Pop().ToNumber();
            bool result = operation(op1, op2);
            this.env.Stack.Push(new BooleanValue(result));
        }
    }
}