using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class InstructionTest
    {
        [Fact]
        public void TestValidInstructions()
        {
            Instruction i = new Instruction(null, "nop", null, null);
            Assert.Equal("    nop", i.ToDisplayString());
            i = new Instruction("init", "nop", null, null);
            Assert.Equal("init:\n    nop", i.ToDisplayString());
            i = new Instruction(null, "br", new StringValue("init"), null);
            Assert.Equal("    br init", i.ToDisplayString());
            i = new Instruction(null, "store_arr", new StringValue("a"), new NumberValue(2));
            Assert.Equal("    store_arr a 2", i.ToDisplayString());
        }

        [Fact]
        public void TestInvalidInstructions()
        {
            Assert.False(Instruction.IsValid("invalid", null, null));
            Assert.False(Instruction.IsValid(null, null, null));
            Assert.False(Instruction.IsValid("nop", new NumberValue(0), null));
            Assert.False(Instruction.IsValid("br", new NumberValue(0), null));
            Assert.False(Instruction.IsValid("br_if", null, new StringValue("x")));
            Assert.False(Instruction.IsValid("load_arr", new StringValue("a"), new BooleanValue(false)));
        }
    }
}
