using Xunit;
using ISB.Scanner;

namespace ISB.Tests
{
    public class TextRangeTest
    {
        [Fact]
        public void Test1()
        {
            TextRange range = ((0, 0), (0, 1));
            Assert.Equal((0, 0), range.Start);
            Assert.Equal((0, 1), range.End);
        }

        [Fact]
        public void Test2()
        {
            TextRange range1 = ((0, 0), (0, 1));
            TextRange range2 = ((0, 0), (0, 1));
            TextRange range3 = ((0, 1), (0, 1));
            Assert.True(range1 == range2);
            Assert.True(range1 != range3);
            Assert.True(range1.Contains((0, 0)));
            Assert.False(range3.Contains((0, 0)));
            Assert.True(range3.Contains((0, 1)));
        }

        [Fact]
        public void Test3()
        {
            TextRange range = ((1, 2), (3, 4));
            Assert.Equal(((1^2)^(3^4)), range.GetHashCode());
            Assert.Equal("((1, 2), (3, 4))", range.ToDisplayString());
        }
    }
}
