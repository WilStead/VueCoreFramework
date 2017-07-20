using System.Globalization;

namespace VueCoreFramework.Core.Configuration
{
    /// <summary>
    /// Contains localization configuration data.
    /// </summary>
    public static class LocalizationConfig
    {
        /// <summary>
        /// Retrieves a list of the <see cref="CultureInfo"/> objects supported by this framework.
        /// </summary>
        public static CultureInfo[] SupportedCultures => new[]
            {
                new CultureInfo("en-US")
            };
    }
}
