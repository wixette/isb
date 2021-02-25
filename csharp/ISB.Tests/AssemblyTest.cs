using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class AssemblyTest
    {
        [Fact]
        public void TestFormatAndParse()
        {
            Assembly assembly1 = new Assembly();
            assembly1.AddInstruction(new Instruction(null, "nop", null, null));
            assembly1.AddInstruction(new Instruction("label1", "add", null, null));
            assembly1.AddInstruction(new Instruction("label2", "sub", null, null));
            assembly1.AddInstruction(new Instruction("label3", "br", new StringValue("label1"), null));
            assembly1.AddInstruction(new Instruction(null, "br_if", new StringValue("label2"), new StringValue("label3")));
            assembly1.AddInstruction(new Instruction(null, "store_arr", new StringValue("a"), new NumberValue(2)));

            string asm = assembly1.ToTextFormat();
            Assert.Equal(@"    nop
label1:
    add
label2:
    sub
label3:
    br label1
    br_if label2 label3
    store_arr a 2
", asm);

            Assembly assembly2 = Assembly.Parse(asm);
            Assert.Equal(6, assembly2.InstructionSequence.Count);
            Assert.Equal("nop", assembly2.InstructionSequence[0].Name);
            Assert.Equal("label1", assembly2.InstructionSequence[1].Label);
            Assert.Equal("label3", assembly2.InstructionSequence[4].Oprand2.ToString());
            Assert.Equal("2", assembly2.InstructionSequence[5].Oprand2.ToString());
        }
    }
}
