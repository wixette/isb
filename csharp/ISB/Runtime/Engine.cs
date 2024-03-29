// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
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
        private Stack<int> stackFrameBases = new Stack<int>();
        private bool pausingCoroutine = false;
        private bool isCoroutineRunning = false;

        public Engine(string moduleName,
            IEnumerable<Type> externalLibClasses=null,
            IEnumerable<Type> disabledLibClasses=null)
        {
            this.env = new Environment(externalLibClasses, disabledLibClasses);
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

        public TextRange CurrentSourceTextRange =>
            IP >= 0 && IP < assembly.Instructions.Count ? assembly.SourceMap[IP] : TextRange.None;

        public int StackCount => this.env.RuntimeStack.Count;

        public BaseValue StackTop =>
            this.env.RuntimeStack.Count > 0 ? (BaseValue)this.env.RuntimeStack.Peek().Clone() : null;

        public BaseValue StackPop() =>
            this.env.RuntimeStack.Count > 0 ? (BaseValue)this.env.RuntimeStack.Pop() : null;

        public string LibsHelpString => this.env.Libs.GetHelpString();

        public void Reset()
        {
            if (isCoroutineRunning)
            {
                this.ReportCoroutineReentered();
                return;
            }
            this.env.Reset();
            this.assembly.Clear();
            this.CodeLines.Clear();
        }

        // Compiles an ISB source code. Returns false if there are compile errors.
        public bool Compile(string code, bool reset = true)
        {
            if (isCoroutineRunning)
            {
                this.ReportCoroutineReentered();
                return false;
            }
            if (reset)
            {
                // A full reset for the compilation.
                this.Reset();
            }
            this.diagnostics.Reset();
            var tokens = Scanner.Scan(code, diagnostics);
            var tree = Parser.Parse(tokens, diagnostics);
            if (this.HasError)
            {
                return false;
            }
            Assembly newAssembly =
                Compiler.Compile(tree, this.env, this.moduleName, this.diagnostics,
                                 out HashSet<string> newCompileTimeLabels,
                                 out HashSet<string> newCompileTimeSubNames);
            if (this.HasError)
            {
                return false;
            }
            int baseSourceLineNo = this.CodeLines.Count;
            MergeAssembly(this.assembly, newAssembly, baseSourceLineNo);
            MergeCodeLines(this.CodeLines, Scanner.SplitCodeToLines(code));
            MergeSymbolSet(this.env.CompileTimeLabels, newCompileTimeLabels);
            MergeSymbolSet(this.env.CompileTimeSubNames, newCompileTimeSubNames);
            return true;
        }

        // Parsing assembly does not support incremental mode. Once a new assembly is parsed, the
        // existing instructions and environment states are cleared.
        public void ParseAssembly(string assemblyCode)
        {
            if (isCoroutineRunning)
            {
                this.ReportCoroutineReentered();
                return;
            }
            this.env.Reset();
            this.CodeLines.Clear();
            this.diagnostics.Reset();
            // TODO: checks and reports errors. (the current implementation simply ignores parsing
            // errors.)
            this.assembly = Compiler.ParseAssembly(assemblyCode);
        }

        // Runs a compiled program until the end. Returns false if there are runtime errors.
        public bool Run(bool reset = true)
        {
            if (isCoroutineRunning)
            {
                this.ReportCoroutineReentered();
                return false;
            }
            BeforeExecution(reset);
            this.ExecuteAssembly();
            return !this.HasError;
        }

        // Runs a compiled program as in a coroutine context.
        //
        // The process yields with an integer instruction counter every time numInstructionsPerStep
        // instructions are executed.
        //
        // If doneCallback is not null, the callback action will be invoked when the execution is
        // done, with the success flag as the only argument.
        //
        // If stepCallback is not null, the callback function will be invoked every step. An
        // instruction counter will be fed into the callback. If stepCallback returns false to
        // the engine, the coroutine will be cancelled immediately.
        //
        // If the execution has been cancelled due to a false return value of canContinueCallback,
        // the ISB engine will have a runtime error info to record that.
        //
        // This can be used in Unity to build a run-per-frame scripting sandbox, with the ability to
        // check and cancel a program if it contains time-consuming logic such as an infinite loop.
        public IEnumerator RunAsCoroutine(Action<bool> doneCallback = null,
                                          Func<int, bool> stepCallback = null,
                                          bool reset = true,
                                          int maxInstructionsPerStep = 10000)
        {
            if (isCoroutineRunning)
            {
                this.ReportCoroutineReentered();
                yield break;
            }
            isCoroutineRunning = true;
            pausingCoroutine = false;
            BeforeExecution(reset);
            int counter = 0;
            while (true)
            {
                counter +=
                    this.ExecuteAssemblyForOneStep(maxInstructionsPerStep);
                if (this.HasError || this.env.IP >= this.assembly.Instructions.Count)
                {
                    break;
                }
                else if (!(stepCallback is null) && !stepCallback(counter))
                {
                    this.ReportCoroutineCancelled();
                    break;
                }
                else
                {
                    while (pausingCoroutine)
                    {
                        yield return counter;
                    }
                }
            }
            if (!(doneCallback is null))
            {
                doneCallback(!this.HasError);
            }
            isCoroutineRunning = false;
        }

        // Pauses the coroutine started by RunAsCoroutine, until ResumeCoroutine is called.
        public void PauseCoroutine() {
            pausingCoroutine = true;
        }

        // Resumes the coroutine started by RunAsCoroutine.
        public void ResumeCoroutine() {
            pausingCoroutine = false;
        }

        // Scans for new labels and updates the label dictionary.
        private void BeforeExecution(bool reset)
        {
            if (reset)
            {
                // The env is reset while the compilation result (assembly, codelines) is kept.
                this.env.Reset();
                this.stackFrameBases.Clear();
            }
            this.diagnostics.Reset();
            for (int i = this.env.IP; i < this.assembly.Instructions.Count; i++)
            {
                var instruction = this.assembly.Instructions[i];
                if (instruction.Label != null)
                {
                    this.env.RuntimeLabels[instruction.Label] = i;
                }
            }
        }

        private void ExecuteAssembly()
        {
            // Executes instructions one by one.
            while (this.env.IP < this.assembly.Instructions.Count && !this.HasError)
            {
                ExecuteNextInstruction();
            }
        }

        private int ExecuteAssemblyForOneStep(int maxInstructionsPerStep)
        {
            // Executes at most numInstructionsPerStep instructions.
            int count = 0;
            while (this.env.IP < this.assembly.Instructions.Count &&
                   count < maxInstructionsPerStep &&
                   !pausingCoroutine &&
                   !this.HasError)
            {
                ExecuteNextInstruction();
                count++;
            }
            return count;
        }

        private void ExecuteNextInstruction()
        {
            Instruction instruction = this.assembly.Instructions[this.env.IP];
            try
            {
                ExecuteInstruction(instruction);
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException is OverflowException)
                    this.ReportOverflow(e.Message);
                else
                    this.ReportRuntimeError(e.InnerException.Message);
            }
            catch (OverflowException e)
            {
                this.ReportOverflow(e.Message);
            }
            catch (StackOverflowException e)
            {
                this.ReportStackOverflow(e.Message);
            }
        }

        private static void MergeCodeLines(List<string> to, string[] from)
        {
            to.AddRange(from);
        }

        private static void MergeAssembly(Assembly to, Assembly from, int baseSourceLineNo)
        {
            to.Append(from, baseSourceLineNo);
        }

        private static void MergeSymbolSet(HashSet<string> to, HashSet<string> from)
        {
            foreach (var k in from)
                to.Add(k);
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

        private void ReportCoroutineReentered()
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"A coroutine is already running in this engine.");
        }

        private void ReportCoroutineCancelled()
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"The coroutine has been cancelled.");
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

        private void ReportNoLibPropertyFound(string libName, string propertyName)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"No lib property found, {libName}.{propertyName}");
        }

        private void ReportReadonlyLibProperty(string libName, string propertyName)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Lib property {libName}.{propertyName} is readonly.");
        }

        private void ReportFailedToSetLibProperty(string libName, string propertyName)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Failed to set lib property, {libName}.{propertyName}");
        }

        private void ReportNoLibFunctionFound(string libName, string functionName)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"No lib method found, {libName}.{functionName}");
        }

        private void ReportFailedToCallLibFunction(string libName, string functionName)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Failed to call lib function, {libName}.{functionName}");
        }

        private void ReportOverflow(string description)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Overflow: {description}");
        }

        private void ReportStackOverflow(string description)
        {
            // TODO: moves this message to Resources.
            this.ReportRuntimeError($"Stack overflow: {description}");
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

                case Instruction.BR:
                {
                    string label = instruction.Operand1.ToString();
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
                    string label1 = instruction.Operand1.ToString();
                    int target1 = this.env.RuntimeLabelToIP(label1);
                    string label2 = instruction.Operand2.ToString();
                    int target2 = this.env.RuntimeLabelToIP(label2);
                    if (target1 < 0)
                    {
                        this.ReportUndefinedAssemblyLabel(label1);
                        break;
                    }
                    if (target2 < 0)
                    {
                        this.ReportUndefinedAssemblyLabel(label2);
                        break;
                    }
                    if (this.env.RuntimeStack.Count > 0)
                    {
                        var value = this.env.RuntimeStack.Pop();
                        this.env.IP = value.ToBoolean() ? target1 : target2;
                    }
                    else
                    {
                        this.ReportEmptyStack();
                    }
                    break;
                }

                case Instruction.SET:
                {
                    if (this.env.RuntimeStack.Count > 0)
                    {
                        var value = this.env.RuntimeStack.Pop();
                        this.env.SetRegister(instruction.Operand1, value);
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
                    var value = this.env.GetRegister(instruction.Operand1);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.STORE:
                {
                    if (this.env.RuntimeStack.Count > 0)
                    {
                        var value = this.env.RuntimeStack.Pop();
                        this.env.SetMemory(instruction.Operand1, value);
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
                    var value = this.env.GetMemory(instruction.Operand1);
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
                    var value = instruction.Operand1;
                    Debug.Assert(value is NumberValue);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.PUSHS:
                {
                    var value = instruction.Operand1;
                    Debug.Assert(value is StringValue);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.PUSHB:
                {
                    var value = instruction.Operand1;
                    Debug.Assert(value is BooleanValue);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.CALL:
                {
                    string subLabel = instruction.Operand1.ToString();
                    int targetIP = this.env.RuntimeLabelToIP(subLabel);
                    if (targetIP < 0)
                    {
                        this.ReportUndefinedAssemblyLabel(subLabel);
                        break;
                    }
                    // Records the base of the stack frame before the subroutine invocation.
                    this.stackFrameBases.Push(this.env.RuntimeStack.Count);
                    this.env.RuntimeStack.Push(new NumberValue(this.env.IP + 1));
                    this.env.IP = targetIP;
                    break;
                }

                case Instruction.RET:
                {
                    int numArguments = (int)(instruction.Operand1.ToNumber());
                    if (this.env.RuntimeStack.Count < numArguments + 1)
                    {
                        this.ReportEmptyStack();
                        break;
                    }
                    // Pops up intermediate values before getting the return address.
                    int stackFrameBase = this.stackFrameBases.Pop();
                    while (this.env.RuntimeStack.Count > stackFrameBase + 1) {
                        this.env.RuntimeStack.Pop();
                    }
                    int targetIP = (int)(this.env.RuntimeStack.Pop().ToNumber());
                    for (int i = 0; i < numArguments; i++)
                        this.env.RuntimeStack.Pop();
                    this.env.IP = targetIP;
                    break;
                }

                case Instruction.CALL_LIB:
                {
                    string libName = instruction.Operand1.ToString();
                    string functionName = instruction.Operand2.ToString();
                    if (!this.env.Libs.HasFunction(libName, functionName))
                    {
                        this.ReportNoLibFunctionFound(libName, functionName);
                        break;
                    }
                    int numArguments = env.Libs.GetArgumentNumber(libName, functionName);
                    if (this.env.RuntimeStack.Count < numArguments)
                    {
                        this.ReportEmptyStack();
                        break;
                    }
                    List<BaseValue> arguments = new List<BaseValue>();
                    for (int i = 0; i < numArguments; i++)
                    {
                        arguments.Add(this.env.RuntimeStack.Pop());
                    }
                    if (!this.env.Libs.InvokeFunction(libName, functionName,
                        numArguments > 0 ? arguments.ToArray() : null,
                        out BaseValue retValue))
                    {
                        this.ReportFailedToCallLibFunction(libName, functionName);
                        break;
                    }
                    if (this.env.Libs.HasReturnValue(libName, functionName) && retValue != null)
                    {
                        this.env.RuntimeStack.Push(retValue);
                    }
                    this.env.IP++;
                    break;
                }

                case Instruction.STORE_LIB:
                {
                    string libName = instruction.Operand1.ToString();
                    string propertyName = instruction.Operand2.ToString();
                    if (!this.env.Libs.HasProperty(libName, propertyName))
                    {
                        this.ReportNoLibPropertyFound(libName, propertyName);
                        break;
                    }
                    if (!this.env.Libs.IsPropertyWritable(libName, propertyName))
                    {
                        this.ReportReadonlyLibProperty(libName, propertyName);
                        break;
                    }
                    if (this.env.RuntimeStack.Count <= 0)
                    {
                        this.ReportEmptyStack();
                        break;
                    }
                    BaseValue value = this.env.RuntimeStack.Pop();
                    if (!this.env.Libs.SetPropertyValue(libName, propertyName, value))
                    {
                        this.ReportFailedToSetLibProperty(libName, propertyName);
                        break;
                    }
                    this.env.IP++;
                    break;
                }

                case Instruction.LOAD_LIB:
                {
                    string libName = instruction.Operand1.ToString();
                    string propertyName = instruction.Operand2.ToString();
                    if (!this.env.Libs.HasProperty(libName, propertyName))
                    {
                        this.ReportNoLibPropertyFound(libName, propertyName);
                        break;
                    }
                    BaseValue value = this.env.Libs.GetPropertyValue(libName, propertyName);
                    this.env.RuntimeStack.Push(value);
                    this.env.IP++;
                    break;
                }

                case Instruction.ADD:
                {
                    if (this.BinaryAddOrStringConcat())
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
                    if (this.BinaryLogicalEqualOperation())
                        this.env.IP++;
                    break;
                }

                case Instruction.NE:
                {
                    if (this.BinaryLogicalNotEqualOperation())
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
            int dimension = (int)Math.Floor(instruction.Operand2.ToNumber());
            if (dimension <= 0)
            {
                this.ReportInvalidValue(dimension.ToString());
                arrayNameAndIndices = null;
                return false;
            }
            arrayNameAndIndices = new BaseValue[dimension + 1];
            arrayNameAndIndices[0] = instruction.Operand1;
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

        private bool BinaryLogicalEqualOperation()
        {
            if (this.env.RuntimeStack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            bool result = false;
            BaseValue op2 = this.env.RuntimeStack.Pop();
            BaseValue op1 = this.env.RuntimeStack.Pop();
            string s1 = op1.ToDisplayString();
            string s2 = op2.ToDisplayString();
            if (NumberValue.TryParse(s1, out NumberValue v1) &&
                NumberValue.TryParse(s2, out NumberValue v2))
            {
                result = v1.ToNumber() == v2.ToNumber();
            }
            else
            {
                result = s1 == s2;
            }
            this.env.RuntimeStack.Push(new BooleanValue(result));
            return true;
        }

        private bool BinaryLogicalNotEqualOperation()
        {
            if (this.env.RuntimeStack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            bool result = false;
            BaseValue op2 = this.env.RuntimeStack.Pop();
            BaseValue op1 = this.env.RuntimeStack.Pop();
            string s1 = op1.ToDisplayString();
            string s2 = op2.ToDisplayString();
            if (NumberValue.TryParse(s1, out NumberValue v1) &&
                NumberValue.TryParse(s2, out NumberValue v2))
            {
                result = v1.ToNumber() != v2.ToNumber();
            }
            else
            {
                result = s1 != s2;
            }
            this.env.RuntimeStack.Push(new BooleanValue(result));
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

        private bool BinaryAddOrStringConcat()
        {
            if (this.env.RuntimeStack.Count < 2)
            {
                this.ReportEmptyStack();
                return false;
            }
            BaseValue op2 = this.env.RuntimeStack.Pop();
            BaseValue op1 = this.env.RuntimeStack.Pop();
            if ((op1 is NumberValue || (op1 is StringValue op1Str && op1Str.IsEmpty())) &&
                (op2 is NumberValue || (op2 is StringValue op2Str && op2Str.IsEmpty())))
            {
                var result = op1.ToNumber() + op2.ToNumber();
                this.env.RuntimeStack.Push(new NumberValue(result));
            }
            else
            {
                var result = op1.ToDisplayString() + op2.ToDisplayString();
                this.env.RuntimeStack.Push(new StringValue(result));
            }
            return true;
        }
    }
}
