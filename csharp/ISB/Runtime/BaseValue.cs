// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System.Diagnostics;

namespace ISB.Runtime
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public abstract class BaseValue
    {
        public abstract string ToDisplayString();

        public sealed override string ToString() => this.ToDisplayString();

        internal abstract bool ToBoolean();

        internal abstract decimal ToNumber();
    }
}