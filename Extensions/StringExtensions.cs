namespace MVCCoreVue.Extensions
{
    public static class StringExtensions
    {
        public static string ToInitialCaps(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var chars = str.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        public static string ToInitialLower(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var chars = str.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}
