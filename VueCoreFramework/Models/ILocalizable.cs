using Microsoft.Extensions.Localization;

namespace VueCoreFramework.Models
{
    /// <summary>
    /// If your data classes implement this interface, their ViewModels will automatically invoke
    /// this variation of <see cref="ToString"/> with the current <see cref="IStringLocalizer"/> so
    /// that you can properly localize the result.
    /// </summary>
    interface ILocalizable
    {
        string ToString(IStringLocalizer localizer);
    }
}
