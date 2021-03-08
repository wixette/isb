using Xunit;
using ISB.Scanning;
using ISB.Runtime;

namespace ISB.Tests
{
    public class AssemblyTest
    {
        [Fact]
        public void TestFormatAndParse()
        {
            Assembly assembly1 = new Assembly();
            assembly1.Add(TextRange.None, null, Instruction.NOP, null, null);
            assembly1.Add(TextRange.None, "label1", Instruction.ADD, null, null);
            assembly1.Add(TextRange.None, "label2", Instruction.SUB, null, null);
            assembly1.Add(TextRange.None, "label3", Instruction.BR, "label1", null);
            assembly1.Add(TextRange.None, null, Instruction.BR_IF, "label2", "label3");
            assembly1.Add(TextRange.None, null, Instruction.STORE_ARR, "a", "2");
            assembly1.Add(TextRange.None, null, Instruction.PUSH, Instruction.TrueLiteral, null);
            assembly1.Add(TextRange.None, null, Instruction.PUSHS, "Say \"Hello, World!\"", null);

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
", asm, ignoreLineEndingDifferences: true);

            Assembly assembly2 = Assembly.Parse(asm);
            Assert.Equal(8, assembly2.Instructions.Count);

            Assert.Equal(Instruction.NOP, assembly2.Instructions[0].Name);
            Assert.Equal(Instruction.ADD, assembly2.Instructions[1].Name);
            Assert.Equal(Instruction.SUB, assembly2.Instructions[2].Name);
            Assert.Equal(Instruction.BR, assembly2.Instructions[3].Name);
            Assert.Equal(Instruction.BR_IF, assembly2.Instructions[4].Name);
            Assert.Equal(Instruction.STORE_ARR, assembly2.Instructions[5].Name);
            Assert.Equal(Instruction.PUSH, assembly2.Instructions[6].Name);
            Assert.Equal(Instruction.PUSHS, assembly2.Instructions[7].Name);

            Assert.Equal("label1", assembly2.Instructions[1].Label);
            Assert.Equal("label2", assembly2.Instructions[4].Oprand1.ToString());
            Assert.Equal("label3", assembly2.Instructions[4].Oprand2.ToString());

            Assert.True(assembly2.Instructions[5].Oprand1 is StringValue);
            Assert.Equal("a", assembly2.Instructions[5].Oprand1.ToString());
            Assert.True(assembly2.Instructions[5].Oprand2 is NumberValue);
            Assert.Equal("2", assembly2.Instructions[5].Oprand2.ToString());

            Assert.Equal("Say \"Hello, World!\"", assembly2.Instructions[7].Oprand1.ToString());
        }
    }
}
