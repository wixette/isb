using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ISB.Runtime;

namespace ISB.Benchmark
{
    public class CompilerAndInterpreterBenchmark
    {
        private readonly Random _random = new Random();
        private readonly Engine _engineForCompile = new Engine("BenchmarkCompile");
        private readonly Engine _engineForRun = new Engine("BenchmarkRun1");
        private readonly Engine _engineForRunWithLibCall = new Engine("BenchmarkRun2");

        public CompilerAndInterpreterBenchmark() {
            string code = GenerateCode();
            if (!_engineForRun.Compile(code, true)) {
                throw new Exception(_engineForRun.ErrorInfo.Contents[0].ToDisplayString());
            }
            string codeWithLibCall = GenerateCodeWithLibCall();
            if (!_engineForRunWithLibCall.Compile(codeWithLibCall, true)) {
                throw new Exception(
                    _engineForRunWithLibCall.ErrorInfo.Contents[0].ToDisplayString());
            }
        }

        [Benchmark]
        public int Compile() {
            string code = GenerateCode();
            if (!_engineForCompile.Compile(code, true)) {
                throw new Exception(_engineForCompile.ErrorInfo.Contents[0].ToDisplayString());
            }
            return _engineForCompile.CodeLines.Count;
        }

        [Benchmark]
        public int Run() {
            if (!_engineForRun.Run(true)) {
                throw new Exception(_engineForRun.ErrorInfo.Contents[0].ToDisplayString());
            }
            return _engineForRun.StackCount;
        }

        [Benchmark]
        public int RunWithLibCall() {
            if (!_engineForRunWithLibCall.Run(true)) {
                throw new Exception(
                    _engineForRunWithLibCall.ErrorInfo.Contents[0].ToDisplayString());
            }
            return _engineForRunWithLibCall.StackCount;
        }

        private string GenerateCode() {
            return @$"
a = {_random.Next(1000)}
b = {_random.Next(1000)}
sum = 0
for i = 0 to 1000
  if a > b then
    sum = sum + a
  else
    sum = sum + b
  endif
  a = a + 1
  b = b - 1
endfor
";
        }

        private string GenerateCodeWithLibCall() {
            return @$"
a = {_random.Next(1000)}
b = {_random.Next(1000)}
sum = 0
for i = 0 to 1000
  if a > b then
    sum = sum + a
  else
    sum = sum + b
  endif
  a = a + Math.RandomInt(3)
  b = b - Math.RandomInt(3)
endfor
";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<CompilerAndInterpreterBenchmark>();
        }
    }
}
