using Xunit;
using ISB.Utilities;

namespace ISB.Tests
{
    public class DiagnosticTest
    {
        [Fact]
        public void Test1()
        {
            Diagnostic d = new Diagnostic(DiagnosticCode.GoToUndefinedLabel, ((5, 5), (5, 15)), "unknown");
            Assert.True(d.ToDisplayString().Contains("unknown"));
        }

        [Fact]
        public void Test2()
        {
            DiagnosticBag bag = new DiagnosticBag();
            bag.ReportInvalidExpressionStatement(((5, 5), (5, 15)));
            Assert.Equal(1, bag.Contents.Count);
        }
    }
}
