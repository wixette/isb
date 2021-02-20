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
            Assert.Contains("unknown", d.ToDisplayString());
        }

        [Fact]
        public void Test2()
        {
            DiagnosticBag bag = new DiagnosticBag();
            bag.ReportInvalidExpressionStatement(((5, 5), (5, 15)));
            Assert.Single(bag.Contents);
        }
    }
}
