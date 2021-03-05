using System;
using System.IO;
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
                                CompileToTextFormat(fileName, code, output, Console.Error);
                            }
                        }
                        else
                        {
                            CompileToTextFormat(fileName, code, Console.Out, Console.Error);
                        }
                    }
                    else
                    {
                        RunProgram(fileName, code, Console.Out, Console.Error);
                    }
                }
                else if (ext != null && ext.ToLower() == AssemblyExtension)
                {
                    RunAssembly(fileName, code, Console.Out, Console.Error);
                }
                else
                {
                    Console.Error.WriteLine($"Only {BasicExtension} or {AssemblyExtension} file is supported.");
                }
            }
            else
            {
                // Entering the interactive mode.
            }
        }

        private static bool CompileToTextFormat(string fileName, string code, TextWriter output, TextWriter err)
        {
            Engine engine = new Engine(fileName);
            if (!engine.Compile(code, true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, err);
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

        private static bool RunAssembly(string fileName, string code, TextWriter output, TextWriter err)
        {
            Engine engine = new Engine(fileName);
            engine.ParseAssembly(code);
            if (!engine.Run(true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, err);
                return false;
            }
            if (engine.StackCount > 0)
            {
                output.WriteLine(engine.StackTop.ToDisplayString());
            }
            return true;
        }

        private static bool RunProgram(string fileName, string code, TextWriter output, TextWriter err)
        {
            Engine engine = new Engine(fileName);
            if (!engine.Compile(code, true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, err);
                return false;
            }
            if (!engine.Run(true))
            {
                ErrorReport.Report(code, engine.ErrorInfo, err);
                return false;
            }
            if (engine.StackCount > 0)
            {
                output.WriteLine(engine.StackTop.ToDisplayString());
            }
            return true;
        }
    }
}
