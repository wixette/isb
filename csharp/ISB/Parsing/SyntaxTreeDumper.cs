// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Text;

namespace ISB.Parsing
{
    internal sealed class SyntaxTreeDumper
    {
        public static string Dump(SyntaxNode treeRoot, bool cleanOutput = true)
        {
            StringBuilder sb = new StringBuilder();
            SyntaxTreeDumperVisitor visitor = new SyntaxTreeDumperVisitor(sb, cleanOutput);
            SyntaxTreeWalker walker = new SyntaxTreeWalker(treeRoot);
            walker.Walk(visitor);
            return sb.ToString();
        }

        private class SyntaxTreeDumperVisitor : ISyntaxNodeVisitor
        {
            private StringBuilder sb;
            private bool cleanOutput;

            public SyntaxTreeDumperVisitor(StringBuilder sb, bool cleanOutput)
            {
                this.sb = sb;
                this.cleanOutput = cleanOutput;
            }

            public void VisitNode(SyntaxNode node, int level)
            {
                for (int i = 0; i < level; i++)
                {
                    this.sb.Append("  ");
                }
                if (node.IsTerminator)
                {
                    this.sb.AppendFormat("{0}: {1}", node.Kind.ToDisplayString(),
                        this.cleanOutput ? node.Terminator.Text : node.Terminator.ToDisplayString());
                }
                else
                {
                    this.sb.AppendFormat("{0}", node.Kind.ToDisplayString());
                }
                this.sb.Append("\n");
            }
        }
    }
}