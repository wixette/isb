// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;

namespace ISB.Utilities
{
    [System.AttributeUsage(AttributeTargets.Method | AttributeTargets.Property)]
    public class Doc : Attribute
    {
        public string Content { get; private set; }
        public Doc(string content)
        {
            this.Content = content;
        }
    }
}