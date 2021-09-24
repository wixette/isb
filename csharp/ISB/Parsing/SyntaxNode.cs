// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

// The original parser of Microsoft Small Basic uses a generated class hierarchy
// to represent the tree structure of all kinds of syntax nodes. See
// https://github.com/sb/smallbasic-editor/tree/master/Source/SmallBasic.Generators/Parsing
// for the generating logic. Meanwhile, the parser itself has to understand the
// tree structure for going through a simple top-down algorithm. There is an
// obvious duplicate between the generated class hierarchy and the parser code.

// Here a pure node-based structure is used to represent the syntax tree. It's
// parser's job to know the grammar and ensure the tree structure is valid.

using System.Collections.Generic;
using System.Diagnostics;
using ISB.Scanning;

namespace ISB.Parsing
{
    public class SyntaxNode
    {
        private SyntaxNode()
        {
            this.Kind = SyntaxNodeKind.EmptySyntax;
            this.Children = null;
            this.Parent = null;
            this.Terminator = null;
            this.IsTerminator = true;
        }

        private SyntaxNode(SyntaxNodeKind kind, Token terminator)
        {
            Debug.Assert(kind != SyntaxNodeKind.EmptySyntax);
            this.Kind = kind;
            this.Children = null;
            this.Parent = null;
            this.Terminator = terminator;
            this.IsTerminator = true;
        }

        private SyntaxNode(SyntaxNodeKind kind, in IReadOnlyList<SyntaxNode> children)
        {
            Debug.Assert(kind != SyntaxNodeKind.EmptySyntax);
            Debug.Assert(children != null && children.Count > 0,
                "Non-terminal nodes always have children.");
            this.Kind = kind;
            this.Children = new List<SyntaxNode>(children);
            this.Parent = null;
            foreach (SyntaxNode child in this.Children)
            {
                Debug.Assert(child != null);
                child.Parent = this;
            }
            this.Terminator = null;
            this.IsTerminator = false;
        }

        private SyntaxNode(SyntaxNodeKind kind, params SyntaxNode[] children)
            : this(kind, new List<SyntaxNode>(children))
        {
        }

        // Initializes an empty node, which represents the zero-occurrence
        // case of an optional grammar child.
        public static SyntaxNode CreateEmpty() =>
            new SyntaxNode();

        // Initializes a terminal node.
        public static SyntaxNode CreateTerminal(SyntaxNodeKind kind, Token terminator) =>
            new SyntaxNode(kind, terminator);


        // Initializes a non-terminal node.
        public static SyntaxNode CreateNonTerminal(SyntaxNodeKind kind, in IReadOnlyList<SyntaxNode> children) =>
            new SyntaxNode(kind, children);

        // Initializes a non-terminal node.
        public static SyntaxNode CreateNonTerminal(SyntaxNodeKind kind, params SyntaxNode[] children) =>
            new SyntaxNode(kind, children);

        public bool IsEmpty
        {
            get => (this.Kind == SyntaxNodeKind.EmptySyntax);
        }

        public bool IsTerminator { get; }

        public SyntaxNodeKind Kind { get; }

        public Token Terminator { get; }

        public List<SyntaxNode> Children { get; }

        public SyntaxNode Parent { get; set; }

        public TextRange Range {
            get
            {
                return this.IsEmpty ? ((0, 0), (0, 0)) : (calculateStart(), calculateEnd());

                TextPosition calculateStart()
                {
                    if (this.IsTerminator)
                    {
                        return this.Terminator.Range.Start;
                    }
                    else
                    {
                        return this.Children[0].Range.Start;
                    }
                }

                TextPosition calculateEnd()
                {
                    if (this.IsTerminator)
                    {
                        return this.Terminator.Range.End;
                    }
                    else
                    {
                        return this.Children[this.Children.Count - 1].Range.End;
                    }
                }
            }
        }

        public SyntaxNode FindNodeAt(TextPosition position)
        {
            if (this.IsEmpty || !this.Range.Contains(position))
                return null;

            if (this.Children == null)
                return this;

            foreach (var child in this.Children)
            {
                var result = child.FindNodeAt(position);
                if (result != null)
                    return result;
            }

            return this;
        }
    }
}
