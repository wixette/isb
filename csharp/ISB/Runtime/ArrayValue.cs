// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ISB.Runtime
{
    public sealed class ArrayValue : BaseValue, IReadOnlyDictionary<string, BaseValue>
    {
        private readonly Dictionary<string, BaseValue> contents;

        public ArrayValue()
        {
            this.contents = new Dictionary<string, BaseValue>(StringComparer.OrdinalIgnoreCase);
        }

        // Deep copy from the input dictionary.
        public ArrayValue(IReadOnlyDictionary<string, BaseValue> contents)
            : this()
        {
            foreach (var key in contents.Keys)
            {
                this.contents[key] = (BaseValue)contents[key].Clone();
            }
        }

        public IEnumerable<string> Keys => this.contents.Keys;

        public IEnumerable<BaseValue> Values => this.contents.Values;

        public int Count => this.contents.Count;

        public BaseValue this[string key] => this.contents[key];

        public void SetIndex(string key, BaseValue value)
        {
            this.contents[key] = value;
        }

        public void RemoveIndex(string key)
        {
            if (this.contents.ContainsKey(key))
            {
                this.contents.Remove(key);
            }
        }

        public bool ContainsKey(string key) => this.contents.ContainsKey(key);

        public IEnumerator<KeyValuePair<string, BaseValue>> GetEnumerator() => this.contents.GetEnumerator();

        public bool TryGetValue(string key, out BaseValue value) => this.contents.TryGetValue(key, out value);

        public override string ToDisplayString()
        {
            StringBuilder builder = new StringBuilder();

            void escape(string value)
            {
                foreach (char ch in value)
                {
                    switch (ch)
                    {
                        case ';':
                        case '=':
                        case '\\':
                            builder.Append("\\");
                            break;
                    }
                    builder.Append(ch);
                }
            }

            foreach (KeyValuePair<string, BaseValue> pair in this.contents)
            {
                builder.Append($"{pair.Key}=");
                escape(pair.Value.ToDisplayString());
                builder.Append(";");
            }

            return builder.ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.contents.GetEnumerator();

        public override bool ToBoolean() => false;

        public override decimal ToNumber() => 0;

        public override object Clone()
        {
            return new ArrayValue(this.contents);
        }
    }
}