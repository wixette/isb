using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    public class LibrariesTest
    {
        [Fact]
        void TestProperties()
        {
            Libraries libs = new Libraries();
            Assert.True(libs.HasProperty("Math", "Pi"));
            Assert.True(libs.HasProperty("Math", "PI"));
            Assert.True(libs.HasProperty("math", "pi"));
            Assert.True(libs.HasProperty("MATH", "PI"));

            Assert.False(libs.IsPropertyWritable("math", "pi"));
            decimal pi = libs.GetPropertyValue("math", "pi").ToNumber();
            Assert.Equal(3.14159m, pi, 4);
        }

        [Fact]
        void TestFunctions()
        {
            Libraries libs = new Libraries();
            Assert.True(libs.HasBuiltInFunction("Print"));
            Assert.True(libs.HasBuiltInFunction("PRINT"));
            Assert.True(libs.HasBuiltInFunction("PRINT"));
            Assert.False(libs.HasReturnValue("print"));
            Assert.Equal(1, libs.GetArgumentNumber("print"));

            Assert.True(libs.HasFunction("Math", "Sin"));
            Assert.True(libs.HasFunction("math", "sin"));
            Assert.True(libs.HasReturnValue("math", "sin"));
            Assert.Equal(1, libs.GetArgumentNumber("math", "sin"));

            BaseValue[] parameters = new BaseValue[] { new NumberValue(3) };
            Assert.True(libs.InvokeFunction("math", "sin", parameters, out BaseValue ret1));
            Assert.Equal(0.14112m, ret1.ToNumber(), 4);

            Assert.True(libs.InvokeFunction("math", "random", null, out BaseValue ret2));
            Assert.True(0 <= ret2.ToNumber() && ret2.ToNumber() < 1);
        }
    }
}