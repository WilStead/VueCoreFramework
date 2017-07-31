using Microsoft.Extensions.Localization;

namespace VueCoreFramework.Core.Models
{
    /// <summary>
    /// If your data classes implement this interface, their ViewModels will automatically invoke
    /// this variation of <see cref="ToString"/> with the current <see cref="IStringLocalizer"/> so
    /// that you can properly localize the result.
    /// </summary>
    public interface ILocalizable
    {
        /// <summary>
        /// Returns a localized representation of the object, as a <see cref="string"/>.
        /// </summary>
        /// <param name="localizer">An <see cref="IStringLocalizer"/> instance.</param>
        /// <returns>A localized representation of the object, as a <see cref="string"/>.</returns>
        string ToString(IStringLocalizer localizer);
    }
}
