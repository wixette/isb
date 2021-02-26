// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;

namespace ISB.Runtime
{
    public sealed class Environment
    {
        public Dictionary<string, int> LabelDictionary { get; private set; }

        public Dictionary<string, int> SubModuleNameDictionary { get; private set; }

        public Environment()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.LabelDictionary = new Dictionary<string, int>();
            this.SubModuleNameDictionary = new Dictionary<string, int>();
        }
    }
}