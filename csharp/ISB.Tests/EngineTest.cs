using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class EngineTest
    {
        [Fact]
        public void TestNop()
        {
            Engine engine = new Engine();
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
            Engine engine = new Engine();
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
            Engine engine = new Engine();
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
            Engine engine = new Engine();
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
            Engine engine = new Engine();
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
        public void TestBinaryOprations()
        {
            Engine engine = new Engine();
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
        }


        [Theory]

        [InlineData (@"br abc",
        @"Runtime error: Undefined assembly label, abc (0:     br abc)")]

        [InlineData (@"push 0
        add",
        @"Runtime error: Unexpected empty stack. (1:     add)")]
        public void TestRuntimeErrors(string code, string error)
        {
            Engine engine = new Engine();
            engine.ParseAssembly(code);
            engine.Run(true);
            Assert.True(engine.HasError);
            Assert.Single(engine.ErrorInfo.Contents);
            System.Console.WriteLine(engine.ErrorInfo.Contents[0].ToDisplayString());
            Assert.Equal(error, engine.ErrorInfo.Contents[0].ToDisplayString());
        }
    }
}