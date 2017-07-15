using Microsoft.Extensions.Localization;

namespace VueCoreFramework.Models
{
    interface ILocalizable
    {
        string ToString(IStringLocalizer localizer);
    }
}
