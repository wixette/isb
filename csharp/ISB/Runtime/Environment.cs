// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;

namespace ISB.Runtime
{
    public sealed class Environment
    {
        public Dictionary<string, int> LabelDictionary { get; private set; }

        public Environment()
        {
            this.LabelDictionary = new Dictionary<string, int>();
        }
    }
}