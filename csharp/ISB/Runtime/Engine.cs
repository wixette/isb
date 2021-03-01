// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

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

        public void ParseAssembly(string assemblyCode)
        {
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
                    break;

                case Instruction.PAUSE:
                    // TODO
                    break;

                case Instruction.BR:
                    int target = this.env.LookupLabel(instruction.Oprand1.ToString());
                    Debug.Assert(target >= 0);
                    this.env.IP = target;
                    break;

                case Instruction.BR_IF:
                    int target1 = this.env.LookupLabel(instruction.Oprand1.ToString());
                    int target2 = this.env.LookupLabel(instruction.Oprand2.ToString());
                    Debug.Assert(target1 >= 0 && target2 >= 0);
                    var value = this.env.Stack.Pop();
                    if (value.ToBoolean())
                        this.env.IP = target1;
                    else
                        this.env.IP = target2;
                    break;

                case Instruction.SET:
                    // TODO
                    break;

                case Instruction.GET:
                    // TODO
                    break;

                case Instruction.STORE:
                    // TODO
                    break;

                case Instruction.LOAD:
                    // TODO
                    break;

                case Instruction.STORE_ARR:
                    // TODO
                    break;

                case Instruction.LOAD_ARR:
                    // TODO
                    break;

                case Instruction.PUSH:
                    // TODO
                    break;

                case Instruction.PUSHS:
                    // TODO
                    break;

                case Instruction.CALL:
                    // TODO
                    break;

                case Instruction.RET:
                    // TODO
                    break;

                case Instruction.CALL_LIB:
                    // TODO
                    break;

                case Instruction.STORE_LIB:
                    // TODO
                    break;

                case Instruction.LOAD_LIB:
                    // TODO
                    break;

                case Instruction.ADD:
                    // TODO
                    break;

                case Instruction.SUB:
                    // TODO
                    break;

                case Instruction.MUL:
                    // TODO
                    break;

                case Instruction.DIV:
                    // TODO
                    break;

                case Instruction.EQ:
                    // TODO
                    break;

                case Instruction.NE:
                    // TODO
                    break;

                case Instruction.LT:
                    // TODO
                    break;

                case Instruction.GT:
                    // TODO
                    break;

                case Instruction.LE:
                    // TODO
                    break;

                case Instruction.GE:
                    // TODO
                    break;

            }
        }
    }
}