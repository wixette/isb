// This is a derived work of Microsoft Small Basic (https://github.com/sb).
// The original code is licensed under the MIT License.

using System;

namespace ISB.Utilities
{
    public static class ExceptionUtilities
    {
        public static InvalidOperationException UnexpectedValue<TValue>(TValue value)
        {
            return new InvalidOperationException($"Unexpected value '{value}' of type '{typeof(TValue).FullName}'");
        }
    }
}