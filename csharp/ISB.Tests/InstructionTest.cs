using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class InstructionTest
    {
        [Fact]
        public void TestValidInstructions()
        {
            Instruction i = Instruction.Create(null, Instruction.NOP, null, null);
            Assert.Equal("    nop", i.ToDisplayString());
            i = Instruction.Create("init", Instruction.NOP, null, null);
            Assert.Equal("init:\n    nop", i.ToDisplayString());
            i = Instruction.Create(null, Instruction.BR, "init", null);
            Assert.Equal("    br init", i.ToDisplayString());
            i = Instruction.Create(null, Instruction.STORE_ARR, "a", "2");
            Assert.Equal("    store_arr a 2", i.ToDisplayString());
            i = Instruction.Create(null, Instruction.PUSHS, "\"abc\"", null);
            Assert.Equal("    pushs \"\\\"abc\\\"\"", i.ToDisplayString());
        }

        [Fact]
        public void TestInvalidInstructions()
        {
            Assert.Null(Instruction.Create(null, "invalid", null, null));
            Assert.Null(Instruction.Create(null, null, null, null));
            Assert.Null(Instruction.Create(null, Instruction.NOP, "0", null));
            Assert.Null(Instruction.Create(null, Instruction.BR, null, null));
            Assert.Null(Instruction.Create(null, Instruction.BR_IF, null, "x"));
            Assert.Null(Instruction.Create(null, Instruction.LOAD_ARR, "a", ""));
            Assert.Null(Instruction.Create(null, Instruction.PUSHS, "", null));
        }
    }
}
