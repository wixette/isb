using Xunit;
using ISB.Runtime;

namespace ISB.Tests
{
    // Tests ISB language features, especially the features that are different with the original
    // Microsoft Small Basic language.
    public class LanguageFeaturesTest
    {
        [Fact]
        public void TestBooleanLiterals()
        {
            Engine engine = new Engine("Program");
            string code = "true";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "false";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "a = True\na";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());
        }

        [Fact]
        public void TestBooleanExpressions()
        {
            Engine engine = new Engine("Program");
            string code = "True and True";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "TRUE and FALSE";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "TRUE or FALSE";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "FALSE or FALSE";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "(false or true) and (true and (true and true))";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());
        }

        [Fact]
        public void TestImplicitConversionToBoolean()
        {
            Engine engine = new Engine("Program");
            string code = "1 and 0";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "3 and 4";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "\"\" and True";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "\"False\" and \"True\"";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "\"NotEmptyString\" and \"NotEmptyString\"";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());
        }
    }
}
