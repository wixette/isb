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
            assembly1.Add(null, "nop", null, null);
            assembly1.Add("label1", "add", null, null);
            assembly1.Add("label2", "sub", null, null);
            assembly1.Add("label3", "br", new StringValue("label1"), null);
            assembly1.Add(null, "br_if", new StringValue("label2"), new StringValue("label3"));
            assembly1.Add(null, "store_arr", new StringValue("a"), new NumberValue(2));

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
            Assert.Equal(6, assembly2.Instructions.Count);
            Assert.Equal("nop", assembly2.Instructions[0].Name);
            Assert.Equal("label1", assembly2.Instructions[1].Label);
            Assert.Equal("label3", assembly2.Instructions[4].Oprand2.ToString());
            Assert.Equal("2", assembly2.Instructions[5].Oprand2.ToString());
        }
    }
}
