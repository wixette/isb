using ISB.Runtime;
using ISB.UnityIntegration;
using ISB.Utilities;

namespace ISB.Lib
{
    [Preserve]
    public class String
    {
        [Doc("Returns the character at index.")]
        [Preserve]
        public StringValue CharAt(StringValue s, NumberValue index)
        {
            string str = s.ToDisplayString();
            int i = (int)index.ToNumber();
            if (i < 0 || i >= str.Length)
                return StringValue.Empty;
            return new StringValue(str[i].ToString());
        }

        [Doc("Returns the concatenation of two strings.")]
        [Preserve]
        public StringValue Concat(StringValue s1, StringValue s2)
        {
            return new StringValue(s1.ToDisplayString() + s2.ToDisplayString());
        }

        [Doc("Determines whether the input string ends with subStr.")]
        [Preserve]
        public BooleanValue EndsWith(StringValue s, StringValue subStr)
        {
            bool ret = s.ToDisplayString().EndsWith(subStr.ToDisplayString());
            return new BooleanValue(ret);
        }

        [Doc("Returns the zero-based index of the first occurrence of subStr in the input string, or -1 if not found. ")]
        [Preserve]
        public NumberValue IndexOf(StringValue s, StringValue subStr)
        {
            return new NumberValue(s.ToDisplayString().IndexOf(subStr.ToDisplayString()));
        }

        [Doc("Returns the length of the input string.")]
        [Preserve]
        public NumberValue Len(StringValue s)
        {
            return new NumberValue(s.ToDisplayString().Length);
        }

        [Doc("Determines whether the input string starts with subStr.")]
        [Preserve]
        public BooleanValue StartsWith(StringValue s, StringValue subStr)
        {
            return new BooleanValue(s.ToDisplayString().StartsWith(subStr.ToDisplayString()));
        }

        [Doc("Retrieves a substring from the input.")]
        [Preserve]
        public StringValue Substring(StringValue s, NumberValue startIndex, NumberValue length)
        {
            string str = s.ToDisplayString();
            int start = (int)startIndex.ToNumber();
            int len = (int)length.ToNumber();
            if (len < 0 || start < 0 || start >= str.Length || start + len > str.Length)
                return StringValue.Empty;
            return new StringValue(str.Substring(start, len));
        }

        [Doc("Converts the input string to lower case.")]
        [Preserve]
        public StringValue ToLower(StringValue s)
        {
            return new StringValue(s.ToDisplayString().ToLower());
        }

        [Doc("Converts the input string to upper case.")]
        [Preserve]
        public StringValue ToUpper(StringValue s)
        {
            return new StringValue(s.ToDisplayString().ToUpper());
        }
    }
}
