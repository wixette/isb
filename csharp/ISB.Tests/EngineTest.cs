using Xunit;
using System;
using ISB.Runtime;

namespace ISB.Tests
{
    class CustomLib
    {
        public StringValue Name { get; set; }
    }

    public class EngineTest
    {
        [Fact]
        public void TestNop()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"nop");
            Assert.Equal(0, engine.IP);
            Assert.Equal(0, engine.StackCount);
            engine.Run(true);
            Assert.Equal(1, engine.IP);
            Assert.Equal(0, engine.StackCount);
        }

        [Fact]
        public void TestPushValue()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 3.14");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.IP);
            Assert.Equal(1, engine.StackCount);
            NumberValue value = (NumberValue)engine.StackTop;
            Assert.Equal((decimal)3.14, value.ToNumber());

            engine.ParseAssembly(@"pushs ""3.14""");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.IP);
            Assert.Equal(1, engine.StackCount);
            StringValue str = (StringValue)engine.StackTop;
            Assert.Equal("3.14", str.ToString());
        }

        [Fact]
        public void TestBranch()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"br label2
            label1:
            br label3
            label2:
            br label1
            label3:
            nop");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(4, engine.IP);

            engine.ParseAssembly(@"push 1
            br_if label1 label2
            label1:
            push 1
            br label3
            label2:
            push 2
            label3:
            nop");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(6, engine.IP);
            Assert.Equal("1", engine.StackTop.ToString());
        }

        [Fact]
        public void TestRegisters()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 10
            set 0
            push 20
            set 1
            get 0
            get 1
            add");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(7, engine.IP);
            Assert.Equal("30", engine.StackTop.ToString());
        }

        [Fact]
        public void TestMemoryVariables()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 10
            store a
            push 20
            store b
            load a
            load b
            sub");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(7, engine.IP);
            Assert.Equal("-10", engine.StackTop.ToString());
        }

        [Fact]
        public void TestBinaryOperations()
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(@"push 120
            push 20
            push 30
            push 40
            push 50
            add
            sub
            mul
            div");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(9, engine.IP);
            Assert.Equal("-0.1", engine.StackTop.ToString());

            engine.ParseAssembly(@"push 10
            push 20
            ge");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(3, engine.IP);
            Assert.False(engine.StackTop.ToBoolean());
        }

        [Theory]

        [InlineData (@"3 + 4.5", "7.5")]
        [InlineData (@"3 + 0", "3")]
        [InlineData (@"""Hello, "" + ""World!""", "Hello, World!")]
        [InlineData (@"3 + """"", "3")]
        [InlineData (@"3 + """" + 3", "6")]
        [InlineData (@"""3"" + """"", "3")]
        [InlineData (@"""3"" + """" + 3", "33")]
        [InlineData (@""""" + 3", "3")]
        [InlineData (@""""" + 3 + 3", "6")]
        [InlineData (@""""" + 3 + ""3""", "33")]
        [InlineData (@"""Ret: "" + (3 + 3)", "Ret: 6")]
        [InlineData (@"True + False", "TrueFalse")]
        [InlineData (@"True + 3", "True3")]
        [InlineData (@"""Ret: "" + True", "Ret: True")]
        [InlineData (@"""Ret: "" + (20 > 30)", "Ret: False")]
        public void TestAddOrStringConcatenation(string code, string result)
        {
            Engine engine = new Engine("Program");
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(result, engine.StackTop.ToString());
        }

        [Fact]
        public void TestArrayOperations()
        {
            Engine engine = new Engine("Program");
            engine.Compile(@"a[0] = 3
            a[1] = ""Hello""
            a[1]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("Hello", engine.StackTop.ToString());

            engine.Compile(@"a[0] = 3
            a[1] = ""Hello""
            a[0]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("3", engine.StackTop.ToString());

            engine.Compile(@"a[0] = 3
            a[1] = ""Hello""
            a[""unknown""]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("", engine.StackTop.ToString());

            engine.Compile(@"a[1][2] = 1
            a[8][0] = 2
            a[""a""][1][""b""] = 3
            a[8][0]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("2", engine.StackTop.ToString());

            engine.Compile(@"a[1][2] = 1
            a[8][0] = 2
            a[""a""][1][""b""] = 3
            a[""a""][1][""b""]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("3", engine.StackTop.ToString());

            engine.Compile(@"a[1][2] = 1
            a[8][0] = 2
            a[""a""][1][""b""] = 3
            a[""a""][1][""c""]", true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("", engine.StackTop.ToString());
        }

        [Fact]
        public void TestSubInvocation()
        {
            Engine engine = new Engine("Program");
            engine.Compile(@"a = 10
            b = 20
            abc()
            sub abc
              c = a + b
            endsub
            c", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal("30", engine.StackTop.ToString());
        }

        [Fact]
        public void TestSubInvocationWithIntermediateValuesInStack()
        {
            Engine engine = new Engine("Program");
            engine.Compile(@"sub x
            ""test""
            endsub
            x()", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(0, engine.StackCount);

            engine.Reset();
            engine.Compile(@"sub x
            ""test""
            3 + 4
            String.ToLower(""ABC"")
            endsub
            x()", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(0, engine.StackCount);

            engine.Reset();
            engine.Compile(@"sub x
            ""test""
            endsub
            sub y
            x()
            endsub
            sub z
            ""test""
            ""test""
            y()
            ret = String.ToLower(""TEST"")
            endsub
            z()
            ret", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal("test", engine.StackTop.ToString());
        }

        [Fact]
        public void TestLibProperty()
        {
            Engine engine = new Engine("Program");
            engine.Compile(@"Math.Pi", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(3.14159m, engine.StackTop.ToNumber(), 4);
        }

        [Fact]
        public void TestCustomLib()
        {
            Engine engine = new Engine("Program", new Type[] { typeof(CustomLib) });
            engine.Compile(@"customlib.name = ""hello""
            a = customlib.name
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal("hello", engine.StackTop.ToString());
        }

        [Fact]
        public void TestLibFunction()
        {
            Engine engine = new Engine("Program");
            engine.Compile(@"Print(""Testing the built-in function Print()"")", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(0, engine.StackCount);

            engine.Compile(@"a = Math.Sin(3)
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(0.14112m, engine.StackTop.ToNumber(), 4);

            engine.Compile(@"a = Math.Sin(""3"")
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(0.14112m, engine.StackTop.ToNumber(), 4);

            engine.Compile(@"a = Math.Sin(""invalid_number, to_be_taken_as_zero"")
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(0m, engine.StackTop.ToNumber(), 4);

            engine.Compile(@"a = String.Compare(""a"", ""b"")
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(-1, engine.StackTop.ToNumber());

            engine.Compile(@"a = String.Compare(""a"", ""a"")
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(0, engine.StackTop.ToNumber());

            engine.Compile(@"a = String.Compare(""b"", ""a"")
            a", true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(1, engine.StackTop.ToNumber());
        }

        [Fact]
        public void TestFibonacci()
        {
            const string code =
                @"NUM = 20
                  Fib[0] = 0
                  Fib[1] = 1
                  For i = 2 to NUM
                      Fib[i] = Fib[i - 1] + Fib[i - 2]
                  EndFor
                  Fib[20]";
            Engine engine = new Engine("Program");
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal("6765", engine.StackTop.ToString());
        }

        [Fact]
        public void TestIsPrime()
        {
            const string code =
                @"n = 1000117 ' number to be test.
                  IsPrime = 0
                  if n <= 3 then
                    if n > 1 then
                      IsPrime = 1
                      goto TheEnd
                    else
                      IsPrime = 0
                      goto TheEnd
                    endif
                  elseif n mod 2 = 0 or n mod 3 = 0 then
                    IsPrime = 0
                    goto TheEnd
                  else
                    i = 5
                    while i * i <= n
                      if n mod i = 0 or n mod (i + 2) = 0 then
                        IsPrime = 0
                        goto TheEnd
                      endif
                      i = i + 6
                    endwhile
                    IsPrime = 1
                  endif
                  TheEnd:
                  IsPrime";
            Engine engine = new Engine("Program");
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.True(engine.StackTop.ToBoolean());
        }


        [Theory]

        [InlineData (@"br abc",
        @"Runtime error: Undefined assembly label, abc (0:     br abc)")]

        [InlineData (@"push 0
        add",
        @"Runtime error: Unexpected empty stack. (1:     add)")]

        [InlineData (@"push 3
        push 0
        div",
        @"Runtime error: Division by zero. (2:     div)")]
        public void TestRuntimeErrors(string code, string error)
        {
            Engine engine = new Engine("Program");
            engine.ParseAssembly(code);
            engine.Run(true);
            Assert.True(engine.HasError);
            Assert.Single(engine.ErrorInfo.Contents);
            Assert.Equal(error, engine.ErrorInfo.Contents[0].ToDisplayString());
        }

        [Fact]
        public void TestIncrementalExecution()
        {
            Engine engine = new Engine("Program");
            Assert.Equal(0, engine.IP);
            Assert.Equal(0, engine.StackCount);
            Assert.Empty(engine.CodeLines);

            Assert.True(engine.Compile("a = 10", true));
            Assert.True(engine.Run(true));

            Assert.Single(engine.CodeLines);
            Assert.Equal(2, engine.IP);
            Assert.Equal(0, engine.StackCount);

            Assert.True(engine.Compile("b = 20", false));
            Assert.True(engine.Compile(@"if a > b then
  c = 100
else
  c = 200
endif", false));
            Assert.True(engine.Run(false));

            Assert.Equal(7, engine.CodeLines.Count);
            Assert.Equal(14, engine.IP);
            Assert.Equal(0, engine.StackCount);

            Assert.True(engine.Compile("c", false));
            Assert.True(engine.Run(false));

            Assert.Equal(8, engine.CodeLines.Count);
            Assert.Equal(15, engine.IP);
            Assert.Equal(1, engine.StackCount);

            Assert.Equal(200, engine.StackTop.ToNumber());
        }

        [Fact]
        public void TestCoroutineWithoutCallbacks()
        {
            Engine engine = new Engine("Program");
            string code = @"x = 0
            for i = 1 to 10000
              x = x + 1
            endfor
            x";
            Assert.True(engine.Compile(code, true));
            var enumerator = engine.RunAsCoroutine(null, null, true, 10000);
            while (enumerator.MoveNext());
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(10000, engine.StackTop.ToNumber());
        }

        [Fact]
        public void TestCoroutineWithCallbacks()
        {
            Engine engine = new Engine("Program");
            string code = @"x = 0
            for i = 1 to 10000
              x = x + 1
            endfor
            x";
            Assert.True(engine.Compile(code, true));
            Func<int, bool> canContinueCallback = (counter) => true;
            bool isDone = false;
            Action<bool> doneCallback = (state) => isDone = state;
            var enumerator = engine.RunAsCoroutine(doneCallback, canContinueCallback, true, 10000);
            while (enumerator.MoveNext());
            Assert.True(isDone);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(10000, engine.StackTop.ToNumber());
        }

        [Fact]
        public void TestCoroutineAndCancelIt()
        {
            Engine engine = new Engine("Program");
            string code = @"x = 0
            for i = 1 to 10000
              x = x + 1
            endfor
            x";
            Assert.True(engine.Compile(code, true));
            Func<int, bool> canContinueCallback = (counter) => counter >= 100000 ? false : true;
            bool isDone = false;
            Action<bool> doneCallback = (value) => isDone = value;
            var enumerator = engine.RunAsCoroutine(doneCallback, canContinueCallback, true, 10000);
            while (enumerator.MoveNext());
            Assert.False(isDone);
            Assert.True(engine.HasError);
            Assert.Equal(0, engine.StackCount);
        }

        [Theory]

        [InlineData (@"(""a"" = ""a"")", true)]
        [InlineData (@"(""a"" <> ""a"")", false)]
        [InlineData (@"(""a"" = ""b"")", false)]
        [InlineData (@"(""a"" <> ""b"")", true)]
        [InlineData (@"(""3"" = ""3"")", true)]
        [InlineData (@"(""3"" = 3)", true)]
        [InlineData (@"(""3"" = ""4"")", false)]
        [InlineData (@"(""3"" = 4)", false)]
        [InlineData (@"(""3"" > 2)", true)]
        [InlineData (@"(""3"" > ""2"")", true)]
        [InlineData (@"(""3"" > 4)", false)]
        [InlineData (@"(""3"" > ""4"")", false)]
        // The same behavior with Microsoft Small Basic - ">", "<", ">=", "<=" are not doing
        // real string comparisons. They simply convert strings to numbers than compare numbers.
        [InlineData (@"(""b"" > ""a"")", false)]
        [InlineData (@"(""a"" > ""b"")", false)]
        [InlineData (@"(""3"" > ""2.95"")", true)]
        [InlineData (@"(""3"" < ""2.95"")", false)]
        [InlineData (@"(""3"" < ""3.05"")", true)]
        [InlineData (@"(""3"" > ""3.05"")", false)]
        public void TestStringComparisons(string code, bool ret)
        {
            Engine engine = new Engine("Program");
            engine.Compile(code, true);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(ret, engine.StackTop.ToBoolean());
        }
    }
}
