using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class EngineTest
    {
        [Fact]
        public void TestNOP()
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
        public void TestBR()
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
        }

        [Fact]
        public void TestPUSH()
        {
            Engine engine = new Engine();
            engine.ParseAssembly(@"push 3.14");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.IP);
            Assert.Equal(1, engine.StackCount);
            NumberValue value = (NumberValue)engine.StackTop;
            Assert.Equal((decimal)3.14, value.ToNumber());
        }

        [Fact]
        public void TestPUSHS()
        {
            Engine engine = new Engine();
            engine.ParseAssembly(@"pushs ""3.14""");
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.IP);
            Assert.Equal(1, engine.StackCount);
            StringValue value = (StringValue)engine.StackTop;
            Assert.Equal("3.14", value.ToString());
        }

        [Fact]
        public void TestBR_IF()
        {
            Engine engine = new Engine();
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
    }
}