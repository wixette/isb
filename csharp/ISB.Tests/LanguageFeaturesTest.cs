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
        public void TestLogicalOperators()
        {
            Engine engine = new Engine("Program");
            string code = "True and True";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "true and false";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "TRUE or FALSE";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "FALSE or FALSE";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "(false or true) and (true and (true and true))";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "not false";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "((7 > 5) and False) or not (3 <> 3) or not true and true";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
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
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "3 and 4";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "\"\" and True";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "\"False\" and \"True\"";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = "\"NotEmptyString\" and \"NotEmptyString\"";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());
        }

        [Fact]
        public void TestAssignmentOperatorAndLogicalEqualOperator()
        {
            // This test is to make sure that MSB and ISB have the same behavior whether the token
            // "=" is explained as an assignment operator or a logical equal operator. See also
            // https://github.com/wixette/isb/issues/15
            Engine engine = new Engine("Program");
            string code = @"a = 3
            b = (a = 3)
            b";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = @"a = 3
            b = a = 3";
            engine.Compile(code, true);
            Assert.True(engine.HasError);

            engine.Reset();
            code = @"a = 3
            b = 1
            if b = a = 3 then
              b = 10
            endif
            b";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.Equal(1, engine.StackTop.ToNumber());

            engine.Reset();
            code = @"a = 3
            b = 1
            b = a and 3";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = @"a = 3
            b = (a and 3)
            b";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());

            engine.Reset();
            code = @"a = 3
            b = (false and true)
            b";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.False(engine.StackTop.ToBoolean());

            engine.Reset();
            code = @"3 = 3 and 4 = 4";
            engine.Compile(code, true);
            Assert.False(engine.HasError);
            engine.Run(true);
            Assert.False(engine.HasError);
            Assert.Equal(1, engine.StackCount);
            Assert.True(engine.StackTop.ToBoolean());
        }
    }
}
