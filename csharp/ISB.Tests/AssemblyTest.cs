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
            assembly1.Add(null, Instruction.NOP, null, null);
            assembly1.Add("label1", Instruction.ADD, null, null);
            assembly1.Add("label2", Instruction.SUB, null, null);
            assembly1.Add("label3", Instruction.BR, "label1", null);
            assembly1.Add(null, Instruction.BR_IF, "label2", "label3");
            assembly1.Add(null, Instruction.STORE_ARR, "a", "2");
            assembly1.Add(null, Instruction.PUSH, Instruction.TrueLiteral, null);
            assembly1.Add(null, Instruction.PUSHS, "Say \"Hello, World!\"", null);

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
    push 1
    pushs ""Say \""Hello, World!\""""
", asm);

            Assembly assembly2 = Assembly.Parse(asm);
            Assert.Equal(8, assembly2.Instructions.Count);
            Assert.Equal(Instruction.NOP, assembly2.Instructions[0].Name);
            Assert.Equal("label1", assembly2.Instructions[1].Label);
            Assert.Equal("label3", assembly2.Instructions[4].Oprand2.ToString());
            Assert.Equal("2", assembly2.Instructions[5].Oprand2.ToString());
            Assert.Equal("Say \"Hello, World!\"", assembly2.Instructions[7].Oprand1.ToString());
        }
    }
}
