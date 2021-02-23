// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;

using ISB.Parsing;

namespace ISB.Runtime
{
    public sealed class AssemblyGenerator
    {
        public Assembly AssemblyBlock { get; private set; }

        public AssemblyGenerator(string moduleName,
            SyntaxNode treeRoot,
            IDictionary<string, int> labelDictionary,
            int startInstructionNo = 0)
        {
            this.AssemblyBlock = new Assembly();
        }
    }
}