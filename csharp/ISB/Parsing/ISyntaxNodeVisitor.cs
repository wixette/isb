// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

namespace ISB.Parsing
{
    internal interface ISyntaxNodeVisitor
    {
        void VisitNode(SyntaxNode node, int level);
    }
}