// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace ISB.Runtime
{
    public sealed class Environment
    {
        public HashSet<string> LabelsForCompiling { get; private set; }

        public HashSet<string> SubNamesForCompiling { get; private set; }

        public Dictionary<string, int> Labels { get; private set; }

        public Dictionary<string, int> SubNames { get; private set; }

        public Stack<BaseValue> Stack { get; private set; }

        public Libraries Libs { get; private set; }

        public Dictionary<int, BaseValue> Registers { get; private set; }

        public ArrayValue Memory { get; private set; }

        public int IP { get; set; }

        public Environment()
        {
            this.Reset();
        }

        public void Reset()
        {
            this.LabelsForCompiling = new HashSet<string>();
            this.SubNamesForCompiling = new HashSet<string>();
            this.Labels = new Dictionary<string, int>();
            this.SubNames = new Dictionary<string, int>();
            this.Stack = new Stack<BaseValue>();
            this.Libs = new Libraries();
            this.Registers = new Dictionary<int, BaseValue>();
            this.Memory = new ArrayValue();
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

        private int GetRegisterIndex(BaseValue registerNo)
        {
            return (int)Math.Floor(registerNo.ToNumber());
        }

        public void SetRegister(BaseValue registerNo, BaseValue value)
        {
            int index = this.GetRegisterIndex(registerNo);
            this.Registers[index] = value;
        }

        public BaseValue GetRegister(BaseValue registerNo)
        {
            int index = this.GetRegisterIndex(registerNo);
            return this.Registers.ContainsKey(index) ? this.Registers[index] : StringValue.Empty;
        }

        public void SetMemory(BaseValue variableName, BaseValue value)
        {
            string index = variableName.ToString();
            this.Memory.SetIndex(index, value);
        }

        public BaseValue GetMemory(BaseValue variableName)
        {
            string index = variableName.ToString();
            return this.Memory.ContainsKey(index) ? this.Memory[index] : StringValue.Empty;
        }

        public void SetArrayItem(BaseValue[] arrayNameAndIndices, BaseValue value)
        {
            ArrayValue array = this.Memory;
            for (int i = 0; i < arrayNameAndIndices.Length - 1; i++)
            {
                string index = arrayNameAndIndices[i].ToString();
                if (!array.ContainsKey(index) || !(array[index] is ArrayValue))
                    array.SetIndex(index, new ArrayValue());
                array = (ArrayValue)array[index];
            }
            string lastIndex = arrayNameAndIndices[arrayNameAndIndices.Length - 1].ToString();
            array.SetIndex(lastIndex, value);
        }

        public BaseValue GetArrayItem(BaseValue[] arrayNameAndIndices)
        {
            ArrayValue array = this.Memory;
            for (int i = 0; i < arrayNameAndIndices.Length - 1; i++)
            {
                string index = arrayNameAndIndices[i].ToString();
                if (!array.ContainsKey(index) || !(array[index] is ArrayValue))
                    return StringValue.Empty;
                array = (ArrayValue)array[index];
            }
            string lastIndex = arrayNameAndIndices[arrayNameAndIndices.Length - 1].ToString();
            return array.ContainsKey(lastIndex) ? array[lastIndex] : StringValue.Empty;
        }
    }
}