using Xunit;
using ISB.Scanning;

namespace ISB.Tests
{
    public class TextPositionTest
    {
        [Fact]
        public void Test1()
        {
            TextPosition pos = (0, 0);
            Assert.Equal(0, pos.Line);
            Assert.Equal(0, pos.Column);
        }

        [Fact]
        public void Test2()
        {
            TextPosition pos1 = (1, 2);
            TextPosition pos2 = (1, 2);
            TextPosition pos3 = (1, 1);
            Assert.True(pos1 == pos2);
            Assert.True(pos1.Equals(pos2));
            Assert.False(pos1 != pos2);
            Assert.True(pos3 < pos1);
            Assert.True(pos2 > pos3);
            Assert.True(pos3 <= pos1);
            Assert.True(pos2 >= pos3);
        }

        [Fact]
        public void Test3()
        {
            TextPosition pos = (12, 34);
            Assert.Equal(12^34, pos.GetHashCode());
            Assert.Equal("(12, 34)", pos.ToDisplayString());
        }
    }
}
