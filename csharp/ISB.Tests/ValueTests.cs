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

            var cloned = (BooleanValue)value.Clone();
            Assert.Equal(value.ToBoolean(), cloned.ToBoolean());
            Assert.False(object.ReferenceEquals(value, cloned));
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

            var cloned = (NumberValue)value.Clone();
            Assert.Equal(value.ToNumber(), cloned.ToNumber());
            Assert.False(object.ReferenceEquals(value, cloned));
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

            var cloned = (StringValue)value.Clone();
            Assert.Equal(value.ToString(), cloned.ToString());
            Assert.False(object.ReferenceEquals(value, cloned));
        }

        [Fact]
        public void TestParse()
        {
            Assert.Equal((decimal)1, NumberValue.Parse(" 1 ").Value);
            Assert.Equal((decimal)0.3, NumberValue.Parse("0.3").Value);
            Assert.Equal((decimal)0, NumberValue.Parse("abc").Value);

            Assert.True(BooleanValue.ParseBooleanOperand("True").Value);
            Assert.True(BooleanValue.ParseBooleanOperand("true").Value);
            Assert.False(BooleanValue.ParseBooleanOperand("False").Value);
            Assert.False(BooleanValue.ParseBooleanOperand("false").Value);
            Assert.False(BooleanValue.ParseBooleanOperand("1").Value);
            Assert.False(BooleanValue.ParseBooleanOperand("Anything").Value);

            Assert.Equal("abc", StringValue.ParseEscaped("abc").Value);
            Assert.Equal(@"abc""", StringValue.ParseEscaped(@"abc\""").Value);
            Assert.Equal(@"abc\", StringValue.ParseEscaped(@"abc\\").Value);
        }

        [Fact]
        public void TestArray()
        {
            ArrayValue array = new ArrayValue();
            Assert.Empty(array.Keys);
            Assert.Empty(array.Values);
            Assert.False(array.ContainsKey("1"));
            array.SetIndex("1", new StringValue("a"));
            array.SetIndex("2", new StringValue("b"));
            array.SetIndex("x", new StringValue("y"));
            array.RemoveIndex("2");
            Assert.Equal(2, array.Count);
            Assert.Equal("y", array["x"].ToDisplayString());
            Assert.True(array.ContainsKey("1"));
            Assert.Equal(@"1=a;x=y;", array.ToDisplayString());

            var cloned = (ArrayValue)array.Clone();
            Assert.Equal(array.Count, cloned.Count);
            Assert.False(object.ReferenceEquals(array, cloned));
            foreach (var key in array.Keys)
            {
                Assert.True(cloned.ContainsKey(key));
                Assert.Equal(array[key].ToString(), cloned[key].ToString());
                Assert.False(object.ReferenceEquals(array[key], cloned[key]));
            }
        }
    }
}
