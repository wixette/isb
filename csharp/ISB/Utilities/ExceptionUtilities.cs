// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;

namespace ISB.Utilities
{
    public static class ExceptionUtilities
    {
        // For enum extensions to verify value validity.
        public static InvalidOperationException UnexpectedEnumValue<TValue>(TValue value)
        {
            return new InvalidOperationException($"Unexpected enum value '{value}' of type '{typeof(TValue).FullName}'");
        }
    }
}