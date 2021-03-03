using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class EngineTest
    {
        [Fact]
        public void TestInstructions()
        {
            Engine engine = new Engine();
            engine.ParseAssembly(@"nop");
            Assert.Equal(0, engine.IP);
            Assert.Equal(0, engine.StackCount);
            engine.Run(false);
            Assert.Equal(1, engine.IP);
            Assert.Equal(0, engine.StackCount);
        }
    }
}