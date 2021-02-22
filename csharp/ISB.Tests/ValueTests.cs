using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class ValueTest
    {
        [Theory]
        [InlineData(true, "True", 1)]
        [InlineData(false, "False", 0)]
        public void TestBoolean(bool initial, string displayString, decimal number)
        {
            var value = new BooleanValue(initial);
            Assert.Equal(displayString, value.ToDisplayString());
            Assert.Equal(number, value.ToNumber());
        }

        [Theory]
        [InlineData(0, "0", false)]
        [InlineData(2.0, "2", true)]
        [InlineData(6.2e3, "6200", true)]
        [InlineData(-12837.33, "-12837.33", true)]
        public void TestNumber(decimal initial, string displayString, bool boolValue)
        {
            var value = new NumberValue(initial);
            Assert.Equal(displayString, value.ToDisplayString());
            Assert.Equal(boolValue, value.ToBoolean());
        }

        [Theory]
        [InlineData("hello", true, 0)]
        [InlineData("", false, 0)]
        [InlineData("335", true, 335)]
        [InlineData("-0.2", true, -0.2)]
        public void TestString(string initial, bool boolValue, decimal number)
        {
            var value = new StringValue(initial);
            Assert.Equal(boolValue, value.ToBoolean());
            Assert.Equal(number, value.ToNumber());
        }

        [Theory]
        [InlineData("1", "1")]
        [InlineData("TRUE", "True")]
        public void TestStringParse(string initial, string displayString)
        {
            Assert.Equal(displayString, StringValue.Parse(initial).ToDisplayString());
        }

        [Fact]
        public void TestArray()
        {
            ArrayValue array = new ArrayValue();
            Assert.Empty(array.Keys);
            Assert.Empty(array.Values);
            Assert.False(array.ContainsKey("1"));
            array.SetIndex("1", StringValue.Parse("a"));
            array.SetIndex("2", StringValue.Parse("b"));
            array.SetIndex("x", StringValue.Parse("y"));
            array.RemoveIndex("2");
            Assert.Equal(2, array.Count);
            Assert.Equal("y", array["x"].ToDisplayString());
            Assert.True(array.ContainsKey("1"));
            Assert.Equal(@"1=a;x=y;", array.ToDisplayString());
        }
    }
}