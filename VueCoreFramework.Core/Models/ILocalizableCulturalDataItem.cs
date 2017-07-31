using Microsoft.Extensions.Localization;

namespace VueCoreFramework.Core.Models
{
    /// <summary>
    /// Combines <see cref="ICulturalDataItem"/> and <see cref="ILocalizable"/>.
    /// </summary>
    public interface ILocalizableCulturalDataItem
    {
        /// <summary>
        /// Returns a localized representation of the object for the given culture, as a <see cref="string"/>.
        /// </summary>
        /// <param name="culture">A culture name.</param>
        /// <param name="localizer">An <see cref="IStringLocalizer"/> instance.</param>
        /// <returns>A localized representation of the object for the given culture, as a <see cref="string"/>.</returns>
        string ToString(string culture, IStringLocalizer localizer);
    }
}
