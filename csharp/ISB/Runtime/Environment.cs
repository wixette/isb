// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Collections.Generic;

namespace ISB.Runtime
{
    public sealed class Environment
    {
        public Dictionary<string, int> Labels { get; private set; }

        public Dictionary<string, int> SubNames { get; private set; }

        public Stack<BaseValue> Stack { get; private set; }

        public Libraries Libs { get; private set; }

        public Dictionary<string, BaseValue> Registers { get; private set; }

        public Dictionary<string, BaseValue> Memory { get; private set; }

        public int IP { get; set; }

        public Environment()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.Labels = new Dictionary<string, int>();
            this.SubNames = new Dictionary<string, int>();
            this.Stack = new Stack<BaseValue>();
            this.Libs = new Libraries();
            this.Registers = new Dictionary<string, BaseValue>();
            this.Memory = new Dictionary<string, BaseValue>();
            this.IP = 0;
        }

        public int LookupLabel(string label)
        {
            if (!this.Labels.ContainsKey(label))
                return -1;
            else
                return this.Labels[label];
        }

        public int LookupSub(string subName)
        {
            if (!this.SubNames.ContainsKey(subName))
                return -1;
            else
                return this.SubNames[subName];
        }
    }
}