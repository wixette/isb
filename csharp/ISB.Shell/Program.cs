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
                HelpText="ISB source (*.bas) file or ISB assembly (*.asm) file to run. " +
                    "If not set, the interactive shell mode will be on.")]
            public string InputFile { get; set; }

            [Option('c', "compile", Required=false,
                HelpText="Compile ISB source to ISB assembly.")]
            public bool Compile { get; set; }

            [Option('o', "output", Required=false,
                HelpText="Output ISB assembly file when --compile is set. " +
                    "If not set, the assembly will be written to stdout.")]
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
                RunOptions(options);
            });
        }

        static void RunOptions(Options opts)
        {
            if (!String.IsNullOrWhiteSpace(opts.InputFile))
            {
                string ext = Path.GetExtension(opts.InputFile);
                if (ext != null && ext.ToLower() == BasicExtension)
                {
                    if (opts.Compile)
                    {
                        Console.Error.WriteLine($"Compiling {opts.InputFile} to assembly code.");
                        string code = File.ReadAllText(opts.InputFile);
                        if (!String.IsNullOrWhiteSpace(opts.OutputFile))
                        {
                            using (StreamWriter output = new StreamWriter(opts.OutputFile))
                            {
                                Compiler.Compile(code, output, Console.Error);
                            }
                        }
                        else
                        {
                            Compiler.Compile(code, Console.Out, Console.Error);
                        }
                    }
                    else
                    {
                        Console.Error.WriteLine($"Running BASIC program {opts.InputFile}.");
                    }
                }
                else if (ext != null && ext.ToLower() == AssemblyExtension)
                {
                    Console.Error.WriteLine($"Running ISB Assembly program {opts.InputFile}.");
                }
                else
                {
                    Console.Error.WriteLine($"Only {BasicExtension} or {AssemblyExtension} file is supported.");
                }
            }
            else
            {
                Console.Error.WriteLine("Entering the interactive mode.");
            }
        }
    }
}
