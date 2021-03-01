using System;
using System.IO;
using CommandLine;
using CommandLine.Text;

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

        static void RunOptions(Options opts)
        {
            if (!String.IsNullOrWhiteSpace(opts.InputFile))
            {
                string fileName = Path.GetFileName(opts.InputFile);
                string ext = Path.GetExtension(opts.InputFile);
                if (ext != null && ext.ToLower() == BasicExtension)
                {
                    if (opts.Compile)
                    {
                        // Compiles the input file to assembly code.
                        string code = File.ReadAllText(opts.InputFile);
                        if (!String.IsNullOrWhiteSpace(opts.OutputFile))
                        {
                            using (StreamWriter output = new StreamWriter(opts.OutputFile))
                            {
                                Compiler.CompileToTextFormat(fileName, code, output, Console.Error);
                            }
                        }
                        else
                        {
                            Compiler.CompileToTextFormat(fileName, code, Console.Out, Console.Error);
                        }
                    }
                    else
                    {
                        // Runs BASIC program.
                    }
                }
                else if (ext != null && ext.ToLower() == AssemblyExtension)
                {
                    // Runs ISB assembly.
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
    }
}
