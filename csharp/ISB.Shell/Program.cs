using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using ISB.Runtime;

namespace ISB.Shell
{
    class Program
    {
        public class Options
        {
            [Option('i', "input", Required=false,
                HelpText="BASIC file (*.bas) to run/compile, or ISB assembly file (*.asm) to run. " +
                    "If not set, the interactive shell mode will start.")]
            public string InputFile { get; set; }

            [Option('c', "compile", Required=false,
                HelpText="Compile BASIC code to ISB assembly, without running it.")]
            public bool Compile { get; set; }

            [Option('o', "output", Required=false,
                HelpText="Output file path when --compile is set. " +
                    "If not set, the output assembly will be written to stdout.")]
            public string OutputFile { get; set; }
        }

        private const string BasicExtension = ".bas";
        private const string AssemblyExtension = ".asm";

        static void Main(string[] args)
        {
            var parserResult = Parser.Default.ParseArguments<Options>(args);
            var helpText = HelpText.AutoBuild<Options>(parserResult, h => h, e => e);
            parserResult.WithParsed(options =>
            {
                Console.Error.WriteLine(helpText.Heading);
                Console.Error.WriteLine(helpText.Copyright);
                Console.Error.WriteLine();
                RunOptions(options);
            });
        }

        private static void RunOptions(Options opts)
        {
            if (!String.IsNullOrWhiteSpace(opts.InputFile))
            {
                string fileName = Path.GetFileName(opts.InputFile);
                string ext = Path.GetExtension(opts.InputFile);
                string code = File.ReadAllText(opts.InputFile);
                if (ext != null && ext.ToLower() == BasicExtension)
                {
                    if (opts.Compile)
                    {
                        if (!String.IsNullOrWhiteSpace(opts.OutputFile))
                        {
                            using (StreamWriter output = new StreamWriter(opts.OutputFile))
                            {
                                CompileToTextFormat(fileName, code, output);
                            }
                        }
                        else
                        {
                            CompileToTextFormat(fileName, code, Console.Out);
                        }
                    }
                    else
                    {
                        RunProgram(fileName, code);
                    }
                }
                else if (ext != null && ext.ToLower() == AssemblyExtension)
                {
                    RunAssembly(fileName, code);
                }
                else
                {
                    Console.Error.WriteLine($"Only {BasicExtension} or {AssemblyExtension} file is supported.");
                }
            }
            else
            {
                // Starts the interactive shell if not input file is provided.
                StartREPL();
            }
        }

        private static bool CompileToTextFormat(string fileName, string code, TextWriter output)
        {
            Engine engine = new Engine(fileName);
            if (!engine.Compile(code, true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, Console.Error);
                return false;
            }

            string commentLine = ';' + new String('-', 99);
            output.WriteLine(commentLine);
            output.WriteLine($"; The ISB Assembly code generated from {fileName}");
            output.WriteLine($"; The code can be parsed and run by the shell tool of ISB (Interactive Small Basic).");
            output.WriteLine($"; See https://github.com/wixette/isb for more details.");
            output.WriteLine(commentLine);
            output.WriteLine(engine.AssemblyInTextFormat);
            return true;
        }

        private static bool RunAssembly(string fileName, string code)
        {
            Engine engine = new Engine(fileName);
            engine.ParseAssembly(code);
            if (!engine.Run(true))
            {
                ErrorReport.Report(engine.ErrorInfo, Console.Error);
                return false;
            }
            if (engine.StackCount > 0)
            {
                Console.WriteLine(engine.StackTop.ToDisplayString());
            }
            return true;
        }

        private static bool RunProgram(string fileName, string code)
        {
            Engine engine = new Engine(fileName);
            if (!engine.Compile(code, true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, Console.Error);
                return false;
            }
            if (!engine.Run(true))
            {
                ErrorReport.Report(engine.CodeLines, engine.ErrorInfo, Console.Error);
                return false;
            }
            if (engine.StackCount > 0)
            {
                Console.WriteLine(engine.StackTop.ToDisplayString());
            }
            return true;
        }

        private class Evaluator : REPL.IEvaluator
        {
            private Engine engine;
            public Evaluator()
            {
                this.engine = new Engine("Program");
            }

            public REPL.EvalResult Eval(string line, out Task incompleteTask)
            {
                Debug.Assert(line != null);
                incompleteTask = null;
                if (!engine.Compile(line, false))
                {
                    ErrorReport.Report(line, engine.ErrorInfo, Console.Error);
                    return REPL.EvalResult.OK;
                }
                if (!engine.Run(false))
                {
                    ErrorReport.Report(engine.CodeLines, engine.ErrorInfo, Console.Error);
                    return REPL.EvalResult.OK;
                }
                if (engine.StackCount > 0)
                {
                    Console.WriteLine(engine.StackTop.ToDisplayString());
                }
                return REPL.EvalResult.OK;
            }
        }

        private static void StartREPL()
        {
            Evaluator evaluator = new Evaluator();
            REPL repl = new REPL("] ", "> ", evaluator);
            repl.Loop();
        }
    }
}
