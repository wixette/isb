// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;

namespace ISB.Parsing
{
    internal sealed class SyntaxTreeWalker
    {
        public SyntaxNode Tree { get; }

        public SyntaxTreeWalker(SyntaxNode treeRoot)
        {
            this.Tree = treeRoot;
        }

        public void Walk(ISyntaxNodeVisitor visitor)
        {
            int level = 0;
            this.InternalWalk(this.Tree, level, visitor, null);
        }

        public void Walk(ISyntaxNodeVisitor visitor, IEnumerable<SyntaxNodeKind> acceptedNodeKinds)
        {
            int level = 0;
            this.InternalWalk(this.Tree, level, visitor, acceptedNodeKinds);
        }

        private void InternalWalk(SyntaxNode node, int level, ISyntaxNodeVisitor visitor, IEnumerable<SyntaxNodeKind> acceptedNodeKinds)
        {
            if (acceptedNodeKinds == null || acceptedNodeKinds.Contains(node.Kind))
            {
                visitor.VisitNode(node, level);
            }
            if (node.IsTerminator)
            {
                return;
            }
            foreach (var child in node.Children)
            {
                if (child != null)
                {
                    InternalWalk(child, level + 1, visitor, acceptedNodeKinds);
                }
            }
        }
    }
}