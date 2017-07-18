namespace VueCoreFramework.Core.Extensions
{
    /// <summary>
    /// Custom extensions for <see cref="string"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Gets a version of this string with the first character converted to upper case.
        /// </summary>
        public static string ToInitialCaps(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var chars = str.ToCharArray();
            chars[0] = char.ToUpper(chars[0]);
            return new string(chars);
        }

        /// <summary>
        /// Gets a version of this string with the first character converted to lower case.
        /// </summary>
        public static string ToInitialLower(this string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return str;
            var chars = str.ToCharArray();
            chars[0] = char.ToLower(chars[0]);
            return new string(chars);
        }
    }
}
