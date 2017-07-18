using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;

namespace VueCoreFramework.Core.Extensions
{
    /// <summary>
    /// Custom extensions for enum.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Gets the description of the enum value, or the Name of the value if no
        /// explicit description has been set.
        /// </summary>
        /// <returns>
        /// The description, or null if the value is not named (e.g. a combined Flags
        /// value, or a value which is simply incorrect).
        /// </returns>
        public static string GetDescription(Type type, object item, IStringLocalizer localizer)
        {
            string description = null;
            if (item == null)
            {
                return description;
            }
            var attr = item.GetAttribute<DisplayAttribute>();
            if (attr == null)
            {
                try
                {
                    description = Enum.GetName(type, item);
                }
                catch
                {
                    // An exception indicates the value is not defined in the enum.
                    // This is not treated as an error here; a combined Flags value could be the cause.
                    // The original null value is returned.
                }
            }
            else
            {
                description = attr.Name;
            }
            return localizer[description];
        }
    }
}
